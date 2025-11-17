import os
import uuid
import requests
import openai
import time
import re
from dotenv import load_dotenv
from sentence_transformers import SentenceTransformer, CrossEncoder
from transformers import pipeline

from ingest_utils import extract_text, chunk_text, detect_document_type
from pinecone_client import create_index, upsert_vectors, query_index

load_dotenv()

# ================== CONFIGURACIÓN ==================
INDEX_NAME = os.getenv("PINECONE_INDEX", "rag-index")
EMB_MODEL = os.getenv("EMB_MODEL", "all-mpnet-base-v2")

# LLM settings
LLM_PROVIDER = os.getenv("LLM_PROVIDER", "hf_inference")
HF_API_URL = os.getenv("HF_API_URL", "")
HF_MODEL = os.getenv("HF_MODEL", "")
HF_KEY = os.getenv("HF_INFERENCE_API_KEY", "")
OPENAI_KEY = os.getenv("OPENAI_API_KEY", "")

# ================== EMBEDDINGS ==================
emb_model = SentenceTransformer(EMB_MODEL)
VECTOR_DIM = emb_model.get_sentence_embedding_dimension()

def embed_texts(texts):
    vectors = emb_model.encode(texts, show_progress_bar=False, convert_to_numpy=True)
    return vectors.tolist()


# ================== RE-RANKER Y SUMMARIZER ==================
CROSS_ENCODER_MODEL = os.getenv("CROSS_ENCODER_MODEL", "cross-encoder/ms-marco-MiniLM-L-6-v2")
cross_encoder = CrossEncoder(CROSS_ENCODER_MODEL)

USE_LOCAL_SUMMARIZER = os.getenv("USE_LOCAL_SUMMARIZER", "false").lower() == "true"
if USE_LOCAL_SUMMARIZER:
    summarizer = pipeline("summarization", model=os.getenv("SUMMARIZER_MODEL", "google/pegasus-xsum"))
else:
    summarizer = None


# ================== INGESTA (CON DOCUMENT_ID REAL) ==================
def ingest_file_to_pinecone(file_path: str, source_name: str = "upload", chunk_size: int = 500):
    """
    Procesa completamente un documento:
    - Extrae texto
    - Detecta tipo
    - Analiza imágenes si es PDF
    - Genera document_id
    - Chunking -> embeddings -> Pinecone
    - Genera resumen texto + imágenes
    """

    text = extract_text(file_path)
    if not text or not text.strip():
        return {
            "status": "error",
            "error": "No se pudo extraer texto del documento."
        }

    tamaño_archivo = os.path.getsize(file_path)
    filename = os.path.basename(file_path)

    # Detectar tipo documento
    doc_type = detect_document_type(text)

    # GENERAR ID
    document_id = str(uuid.uuid4())

    # ANALIZAR IMÁGENES (SOLO PDF)
    numero_imagenes = 0
    imagenes_metadata = []
    if file_path.lower().endswith(".pdf"):
        numero_imagenes, imagenes_metadata = analyze_pdf_images(file_path)

    # CHUNKING
    chunks = chunk_text(text, chunk_size=chunk_size, chunk_overlap=100)
    vectors = embed_texts(chunks)

    # UPSERT
    upserts = []
    for i, vec in enumerate(vectors):
        chunk_id = str(uuid.uuid4())

        metadata = {
            "source": source_name,
            "chunk_index": i,
            "document_id": document_id,
            "text_excerpt": chunks[i][:600],
            "doc_type": doc_type,
            "filename": filename
        }

        upserts.append((chunk_id, vec, metadata))

    create_index(INDEX_NAME, dim=VECTOR_DIM)
    upsert_vectors(INDEX_NAME, upserts)

    # GENERAR RESUMEN MEJORADO (TEXTO + IMÁGENES)
    resumen_contexto = text[:3500]

    # Añadir una referencia breve de imágenes
    if numero_imagenes > 0:
        resumen_contexto += (
            "\n\nEl documento contiene imágenes relevantes. "
            f"Número total: {numero_imagenes}. "
        )
        # incluir descripciones si existieran
        for img in imagenes_metadata[:5]:
            resumen_contexto += (
                f"\n- Imagen en página {img['page']}, "
                f"tamaño {img['width']}x{img['height']}, "
                f"enlaces: {img['links']}"
            )

    # Generar resumen
    resumen_documento = ""
    try:
        prompt = (
            "Genera un resumen profesional del documento. "
            "Si contiene imágenes, intégralas al resumen. "
            "Máximo 15 líneas.\n\n"
            f"Contenido:\n{resumen_contexto}"
        )

        if HF_API_URL and "router.huggingface.co" in HF_API_URL:
            headers = {"Authorization": f"Bearer {HF_KEY}", "Content-Type": "application/json"}
            body = {"model": HF_MODEL, "messages": [{"role": "user", "content": prompt}]}
            r = requests.post(HF_API_URL, headers=headers, json=body)
            resumen_documento = r.json()["choices"][0]["message"]["content"]
        else:
            resumen_documento = text[:600]

    except Exception as e:
        resumen_documento = f"Error generando resumen: {str(e)}"

    # METADATA COMPLETA PARA EL BACKEND
    archivo_metadata = {
        "document_id": document_id,
        "filename": filename,
        "doc_type": doc_type,
        "chunks": len(chunks),
        "tamaño_archivo": tamaño_archivo,
        "vector_dim": VECTOR_DIM,
        "source": source_name,
        "numero_imagenes": numero_imagenes,
        "imagenes_metadata": imagenes_metadata
    }

    return {
        "status": "ok",
        "filename": filename,
        "document_id": document_id,
        "doc_type": doc_type,
        "contenido_extraido": text[:5000],
        "resumen_documento": resumen_documento,
        "tamaño_archivo": tamaño_archivo,
        "numero_imagenes": numero_imagenes,
        "archivo_metadata_json": archivo_metadata
    }

# ================== RECUPERACIÓN ==================
def retrieve(query: str, top_k=20, doc_type: str = None):
    """
    Recupera chunks de toda la base vectorial.
    Si doc_type se usa, filtra.
    """
    qvec = embed_texts([query])[0]
    pool_k = max(top_k * 4, 50)

    res = query_index(INDEX_NAME, qvec, top_k=pool_k)
    hits = []

    for m in res.get("matches", []):
        meta = m.get("metadata", {}) or {}

        # Filtrar por tipo de documento si corresponde
        if doc_type:
            if meta.get("doc_type") != doc_type:
                continue

        hits.append({
            "id": m.get("id"),
            "score": m.get("score"),
            "metadata": meta
        })

    return sorted(hits, key=lambda x: x["score"], reverse=True)[:top_k]


# ================== RE-RANKING ==================
def rerank(query: str, hits: list, top_k: int = 10):
    if not hits:
        return []

    pairs = [(query, h["metadata"].get("text_excerpt", "")) for h in hits]

    try:
        scores = cross_encoder.predict(pairs)
    except Exception as e:
        print("Cross-encoder error:", e)
        return hits[:top_k]

    for i, h in enumerate(hits):
        h["_rerank_score"] = float(scores[i]) if i < len(scores) else 0.0

    return sorted(hits, key=lambda x: x["_rerank_score"], reverse=True)[:top_k]


# ================== COMPRESIÓN ==================
def compress_context(hits: list, max_chunks: int = 5, group_size: int = 5):
    if not hits:
        return []

    raw_texts = [h["metadata"].get("text_excerpt", "") for h in hits]
    source_infos = [{
        "id": h["id"],
        "chunk_index": h["metadata"].get("chunk_index"),
        "doc_type": h["metadata"].get("doc_type"),
        "document_id": h["metadata"].get("document_id")
    } for h in hits]

    groups = [raw_texts[i:i + group_size] for i in range(0, len(raw_texts), group_size)]
    src_groups = [source_infos[i:i + group_size] for i in range(0, len(source_infos), group_size)]

    compressed = []
    for grp_texts, grp_src in zip(groups, src_groups):
        joined = "\n\n".join(grp_texts)

        if USE_LOCAL_SUMMARIZER and summarizer:
            try:
                out = summarizer(joined, max_length=128, min_length=30, do_sample=False)
                summary = out[0]["summary_text"]
            except:
                summary = joined[:800]
        else:
            summary = joined[:800]
            last_dot = summary.rfind(".")
            if last_dot > 300:
                summary = summary[:last_dot + 1]

        compressed.append({"text": summary.strip(), "source_info": grp_src})

        if len(compressed) >= max_chunks:
            break

    return compressed


# ================== PROMPTS CRM-AWARE ==================
PROMPT_TEMPLATES = {
    "contrato": (
        "Eres un asistente jurídico para un CRM. Usa solo el contexto para responder. "
        "Organiza en cláusulas, obligaciones, vigencia y sanciones. "
        "Cita siempre fragmentos como [chunk_index]."
    ),
    "correo": (
        "Eres un asistente comercial. Resume el correo, identifica intención y acciones sugeridas."
    ),
    "factura": (
        "Eres un asistente financiero. Extrae valores, fechas e impuestos."
    ),
    "pqr": (
        "Eres un agente de servicio al cliente. Clasifica la PQR y propone acciones."
    ),
    "propuesta": (
        "Eres un asistente comercial. Resume entregables, alcance y valor."
    ),
    "documento": (
        "Eres un asistente de CRM. Resume la información y extrae elementos clave."
    ),
}


def build_prompt(question: str, compressed_context: list, doc_type: str):
    base = PROMPT_TEMPLATES.get(doc_type, PROMPT_TEMPLATES["documento"])

    context_parts = []
    for c in compressed_context:
        idxs = [s.get("chunk_index") for s in c["source_info"] if s.get("chunk_index") is not None]
        idxs = ",".join(str(int(x)) for x in idxs) if idxs else ""
        marker = f"[chunks: {idxs}]\n" if idxs else ""
        context_parts.append(f"{marker}{c['text']}")

    full_ctx = "\n\n".join(context_parts)

    prompt = (
        f"{base}\n\n"
        f"Contexto:\n{full_ctx}\n\n"
        f"Pregunta: {question}\n\n"
        "Respuesta estructurada:"
    )
    return prompt


# ================== GENERACIÓN ==================
def generate_answer(question: str, hits: list, doc_type: str):
    reranked = rerank(question, hits, top_k=len(hits))
    compressed = compress_context(reranked)

    prompt = build_prompt(question, compressed, doc_type)

    output = "Sin modelo configurado."

    # --- HuggingFace Router ---
    if HF_API_URL and "router.huggingface.co" in HF_API_URL:
        headers = {"Authorization": f"Bearer {HF_KEY}", "Content-Type": "application/json"}
        body = {
            "model": HF_MODEL,
            "messages": [
                {"role": "system", "content": "Asistente CRM."},
                {"role": "user", "content": prompt}
            ]
        }
        r = requests.post(HF_API_URL, json=body, headers=headers)
        output = r.json()["choices"][0]["message"]["content"]

    # --- HF normal ---
    elif "api-inference.huggingface.co" in HF_API_URL:
        headers = {"Authorization": f"Bearer {HF_KEY}"}
        r = requests.post(HF_API_URL, headers=headers, json={"inputs": prompt})
        out = r.json()
        output = out[0]["generated_text"] if isinstance(out, list) else str(out)

    # --- OpenAI ---
    elif LLM_PROVIDER == "openai" and OPENAI_KEY:
        openai.api_key = OPENAI_KEY
        r = openai.ChatCompletion.create(
            model="gpt-4o-mini",
            messages=[{"role": "user", "content": prompt}]
        )
        output = r.choices[0].message.content

    return output, compressed


# ================== PIPELINE FINAL ==================
def answer_question(question: str, top_k: int = 20):
    # Este detecta el tipo de búsqueda, no el tipo real del documento
    q = question.lower()
    if any(w in q for w in ["cláusula", "contrato"]):
        filter_type = "contrato"
    elif "factura" in q:
        filter_type = "factura"
    elif any(w in q for w in ["correo", "cliente", "mensaje"]):
        filter_type = "correo"
    else:
        filter_type = None  # búsqueda global

    hits = retrieve(question, top_k=top_k, doc_type=filter_type)

    # Si no encontró nada del tipo filtrado → buscar global
    if not hits and filter_type:
        hits = retrieve(question, top_k=top_k, doc_type=None)
        filter_type = "documento"

    answer_raw, compressed = generate_answer(question, hits, filter_type or "documento")

    return {
        "answer": answer_raw,
        "sources": [h["metadata"] for h in hits],
        "compressed": compressed,
        "doc_type_detected": filter_type or "documento"
    }

import pymupdf as fitz
import hashlib
import io

def analyze_pdf_images(path: str, summarize_fn=None):
    """
    Extrae imágenes y metadata visual del PDF.
    summarize_fn: función opcional que recibe image_bytes y devuelve un resumen.
    """
    doc = fitz.open(path)

    image_results = []
    total_images = 0

    for page_num, page in enumerate(doc):
        # Extraer imágenes
        img_list = page.get_images(full=True)

        # Extraer enlaces de la página
        links = page.get_links()
        page_links = [l.get("uri") for l in links if l.get("uri")]

        for img_index, img_info in enumerate(img_list):
            xref = img_info[0]
            base_img = doc.extract_image(xref)

            image_bytes = base_img["image"]
            width = base_img["width"]
            height = base_img["height"]

            sha256 = hashlib.sha256(image_bytes).hexdigest()

            # Intentar capturar bbox: no siempre existe, pero probamos
            try:
                bbox = page.get_image_bbox(img_info)
            except:
                bbox = None

            # Detectar si la imagen coincide con un link visual (heurístico)
            has_link = False
            associated_links = []
            if bbox:
                for l in links:
                    if l.get("uri") and "from" in l and l["from"].intersects(bbox):
                        has_link = True
                        associated_links.append(l["uri"])

            # Resumen visual (opcional)
            resumen = None
            if summarize_fn:
                try:
                    resumen = summarize_fn(image_bytes)
                except:
                    resumen = None

            image_results.append({
                "page": page_num + 1,
                "width": width,
                "height": height,
                "bbox": str(bbox) if bbox else None,
                "sha256": sha256,
                "image_size": len(image_bytes),
                "has_link": has_link,
                "links": associated_links,
                "page_links": page_links,
                "summary": resumen
            })

            total_images += 1

    return total_images, image_results

