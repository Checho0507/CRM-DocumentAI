import os
import shutil
import json
import time
from fastapi import FastAPI, UploadFile, File, Form
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
from pathlib import Path
from dotenv import load_dotenv

from rag_pipeline import ingest_file_to_pinecone, answer_question
from ingest_utils import extract_text, detect_document_type

# ==========================
# CONFIGURACIÓN INICIAL
# ==========================
load_dotenv()

UPLOAD_DIR = Path("uploads")
UPLOAD_DIR.mkdir(exist_ok=True)

app = FastAPI(
    title="CRM RAG Service",
    description="Servicio RAG inteligente para CRM: contratos, correos, facturas, PQRS y propuestas.",
    version="2.1.0"
)

# ==========================
# CORS
# ==========================
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

# ==========================
# MODELOS DE ENTRADA
# ==========================
class Question(BaseModel):
    query: str
    doc_type: str | None = None  # filtro opcional

class Feedback(BaseModel):
    question: str
    answer: str
    doc_type: str | None = None
    correct: bool
    comment: str | None = None

# ==========================
# ENDPOINTS
# ==========================

@app.get("/health")
async def health():
    return {"status": "ok", "message": "CRM RAG Service funcionando correctamente."}

# ---------- INGESTA ----------
@app.post("/upload")
async def upload_document(file: UploadFile = File(...), source_name: str = Form("upload")):
    """Sube e indexa un documento en Pinecone."""
    dest = UPLOAD_DIR / file.filename
    with open(dest, "wb") as out_file:
        shutil.copyfileobj(file.file, out_file)

    result = ingest_file_to_pinecone(str(dest), source_name=source_name)
    return {"upload_status": result, "filename": file.filename}

# ---------- CONSULTAS RAG ----------
@app.post("/query")
async def query_rag(q: Question):
    """
    Consulta al RAG.
    - Si el usuario envía doc_type → se usa como filtro.
    - Si no, el pipeline detecta el tipo automáticamente.
    """
    start = time.time()

    result = answer_question(q.query, top_k=10)

    elapsed = round(time.time() - start, 2)

    return {
        "query": q.query,
        "input_doc_type": q.doc_type,
        "detected_doc_type": result.get("doc_type_detected"),
        "response": result.get("answer"),
        "sources": result.get("sources", []),
        "compressed_context": result.get("compressed", []),
        "elapsed_seconds": elapsed
    }

# ---------- FEEDBACK ----------
FEEDBACK_LOG = Path("feedback_log.json")

@app.post("/feedback")
async def feedback(data: Feedback):
    """Guarda feedback del usuario."""
    record = data.dict()
    record["timestamp"] = time.strftime("%Y-%m-%d %H:%M:%S")

    previous = []
    if FEEDBACK_LOG.exists():
        try:
            previous = json.loads(FEEDBACK_LOG.read_text())
        except:
            previous = []

    previous.append(record)
    FEEDBACK_LOG.write_text(json.dumps(previous, indent=2, ensure_ascii=False))

    return {"status": "feedback_saved", "count": len(previous)}

# ---------- ANALYZE ----------
@app.post("/analyze")
async def analyze_document(file: UploadFile = File(...)):
    """
    Analiza automáticamente un documento sin indexarlo.
    Devuelve:
        - tipo detectado
        - vista previa del contenido
    """
    dest = UPLOAD_DIR / file.filename
    with open(dest, "wb") as out_file:
        shutil.copyfileobj(file.file, out_file)

    text = extract_text(str(dest))
    snippet = text[:1000] + ("..." if len(text) > 1000 else "")

    # Usamos la misma lógica del sistema
    detected = detect_document_type(text)

    return {
        "filename": file.filename,
        "detected_doc_type": detected,
        "text_preview": snippet
    }
