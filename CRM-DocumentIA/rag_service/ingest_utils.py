import email
import docx
import re
from pathlib import Path
from pdfminer.high_level import extract_text as extract_pdf_text
from langchain.text_splitter import RecursiveCharacterTextSplitter

# ============================================================
# 1. EXTRACCIÓN DE TEXTO
# ============================================================

def extract_text_from_pdf(path: str) -> str:
    text = extract_pdf_text(path)
    return clean_text(text)

def extract_text_from_docx(path: str) -> str:
    doc = docx.Document(path)
    text = "\n".join(p.text for p in doc.paragraphs if p.text.strip())
    return clean_text(text)

def extract_text_from_txt(path: str) -> str:
    with open(path, "r", encoding="utf-8") as f:
        text = f.read()
    return clean_text(text)

def extract_text_from_email(path: str) -> str:
    with open(path, "r", encoding="utf-8") as f:
        msg = email.message_from_file(f)

    parts = []
    if msg.is_multipart():
        for part in msg.walk():
            if part.get_content_type() == "text/plain":
                try:
                    parts.append(part.get_payload(decode=True).decode(errors="ignore"))
                except:
                    pass
    else:
        try:
            parts.append(msg.get_payload(decode=True).decode(errors="ignore"))
        except:
            parts.append(msg.get_payload())

    return clean_text("\n".join(parts))

def extract_text(path: str) -> str:
    ext = Path(path).suffix.lower()

    if ext == ".pdf":
        return extract_text_from_pdf(path)
    if ext == ".docx":
        return extract_text_from_docx(path)
    if ext == ".txt":
        return extract_text_from_txt(path)
    if ext == ".eml":
        return extract_text_from_email(path)

    raise ValueError(f"❌ Formato no soportado: {ext}")


# ============================================================
# 2. NORMALIZACIÓN DE TEXTO
# ============================================================

def clean_text(text: str) -> str:
    """Normaliza saltos, espacios, ruido y numeraciones repetidas."""
    if not text:
        return ""

    text = text.replace("\r", " ").replace("\n", " ")
    text = re.sub(r"\s{2,}", " ", text)

    # Quitar numeraciones tipo: 1., 2), a), I.
    text = re.sub(r"(?m)^\s*[\-\*\•]?\s*\d+[\.\)]\s*", "", text)
    text = re.sub(r"(?m)^\s*[a-zA-Z][\.\)]\s*", "", text)

    return text.strip()


# ============================================================
# 3. DETECCIÓN AUTOMÁTICA DEL TIPO DE DOCUMENTO
# ============================================================

DOC_PATTERNS = {
    "contrato": [
        "contrato", "contratante", "contratista", "cláusula", "objeto",
        "honorarios", "vigencia", "obligaciones", "penal"
    ],
    "correo": [
        "asunto:", "from:", "to:", "cc:", "estimado", "saludos",
        "cordialmente", "firma", "enviado"
    ],
    "factura": [
        "factura", "subtotal", "iva", "total", "valor total", "nit",
        "cuenta por pagar", "número de factura"
    ],
    "propuesta": [
        "propuesta", "cotización", "oferta", "valor ofertado",
        "servicios ofrecidos", "entregables", "alcance"
    ],
    "pqr": [
        "petición", "queja", "reclamo", "solicito", "pqr", "radicado"
    ],
    "acta": [
        "acta", "reunión", "acuerdos", "asistentes", "orden del día", "compromisos"
    ],
    "minuta": [
        "minuta", "temas tratados", "seguimiento", "resumen de reunión"
    ]
}


def detect_document_type(text: str) -> str:
    """Clasifica automáticamente según palabras clave."""
    t = text.lower()
    scores = {}

    for doc_type, keywords in DOC_PATTERNS.items():
        scores[doc_type] = sum(1 for k in keywords if k in t)

    best = max(scores, key=scores.get)

    if scores[best] == 0:
        return "documento"

    # Si empatan varias categorías → documento genérico
    if list(scores.values()).count(scores[best]) > 1:
        return "documento"

    return best


# ============================================================
# 4. CHUNKING INTELIGENTE
# ============================================================

def chunk_text(text: str, chunk_size: int = 800, chunk_overlap: int = 200):
    """
    Divide texto en fragmentos semánticos.
    Detecta contratos, correos, facturas o propuestas y los separa por secciones.
    """
    # 1) Contratos → separar por "CLÁUSULA"
    clausulas = re.split(r"(?i)\bcláusula\b|\bclausula\b", text)
    if len(clausulas) > 3:
        chunks = []
        for c in clausulas:
            c = c.strip()
            if len(c) > 60:
                chunks.append(c)
        return chunks

    # 2) Correos → separar por encabezados
    if re.search(r"(?i)(asunto|estimad|saludo|atentamente|firma)", text):
        parts = re.split(r"(?i)(asunto:|para:|cc:|firma:|atentamente|saludos)", text)
        return [p.strip() for p in parts if len(p.strip()) > 80]

    # 3) Facturas → separar por totales
    if re.search(r"(?i)(subtotal|iva|total|factura)", text):
        parts = re.split(r"(?i)(subtotal|iva|total)", text)
        return [p.strip() for p in parts if len(p.strip()) > 80]

    # 4) Propuestas → separar por secciones típicas
    if re.search(r"(?i)(alcance|entregables|condiciones|valor|oferta)", text):
        parts = re.split(r"(?i)(alcance|entregables|condiciones|valor|plazo)", text)
        return [p.strip() for p in parts if len(p.strip()) > 80]

    # 5) fallback → splitter genérico
    splitter = RecursiveCharacterTextSplitter(
        chunk_size=chunk_size,
        chunk_overlap=chunk_overlap,
        separators=["\n\n", "\n", ".", ";", " "]
    )

    docs = splitter.create_documents([text])
    return [d.page_content.strip() for d in docs if d.page_content.strip()]
