import os
import time
from dotenv import load_dotenv
from pinecone import Pinecone, ServerlessSpec

# ==========================================
# CONFIGURACIÓN
# ==========================================
load_dotenv()

API_KEY = os.getenv("PINECONE_API_KEY")
INDEX_NAME = os.getenv("PINECONE_INDEX", "rag-index")

# Región segura por defecto
DEFAULT_REGION = "us-east-1"
DEFAULT_CLOUD = "aws"

# Estándar recomendado por Pinecone 2025
VALID_REGIONS = {
    "aws": ["us-east-1", "eu-west-1"],
    "gcp": ["us-central1"]
}

ENV_REGION = os.getenv("PINECONE_ENV", DEFAULT_REGION).strip()
ENV_CLOUD = os.getenv("PINECONE_CLOUD", DEFAULT_CLOUD).strip()

# Validación automática
if ENV_CLOUD not in VALID_REGIONS or ENV_REGION not in VALID_REGIONS.get(ENV_CLOUD, []):
    print(f"⚠️ Región inválida '{ENV_REGION}' para cloud '{ENV_CLOUD}'. Usando {DEFAULT_CLOUD}/{DEFAULT_REGION} por defecto.")
    ENV_CLOUD = DEFAULT_CLOUD
    ENV_REGION = DEFAULT_REGION

# Inicializar Pinecone client
pc = Pinecone(api_key=API_KEY)


# ==========================================
# CREACIÓN Y OBTENCIÓN DE ÍNDICE
# ==========================================
def create_index(index_name: str, dim: int, metric: str = "cosine"):
    """
    Crea un índice serverless si no existe.
    """

    try:
        existing = pc.list_indexes().names()

        if index_name in existing:
            print(f"ℹ️ El índice '{index_name}' ya existe.")
            return

        print(f"⚙️ Creando índice '{index_name}' en {ENV_CLOUD}/{ENV_REGION}...")

        pc.create_index(
            name=index_name,
            dimension=dim,
            metric=metric,
            spec=ServerlessSpec(
                cloud=ENV_CLOUD,
                region=ENV_REGION
            )
        )

        # Pinecone recomienda esperar unos segundos
        time.sleep(5)

        print(f"✅ Índice '{index_name}' creado correctamente.")

    except Exception as e:
        print(f"❌ Error al crear índice '{index_name}': {e}")


def get_index(index_name: str):
    """Retorna handler del índice."""
    try:
        return pc.Index(index_name)
    except Exception as e:
        raise RuntimeError(f"❌ Error accediendo al índice '{index_name}': {e}")


# ==========================================
# UPSERT
# ==========================================
def upsert_vectors(index_name: str, vectors: list):
    """
    vectors = [(id, vector, metadata), ...]
    """
    try:
        index = get_index(index_name)

        index.upsert(
            vectors=vectors  # new Pinecone SDK format
        )

        print(f"✅ Upsert completado en '{index_name}'. Insertados: {len(vectors)}")

    except Exception as e:
        print(f"❌ Error en upsert: {e}")


# ==========================================
# QUERY
# ==========================================
def query_index(index_name: str, vector, top_k=10, include_metadata=True, filter=None):
    """
    Realiza una búsqueda con vector + filtro opcional.

    Ejemplo de filtro:
        {"doc_type": {"$eq": "contrato"}}
        {"document_id": {"$eq": "doc123"}}
        {"source": {"$eq": "cliente_x"}}
    """

    try:
        index = get_index(index_name)

        res = index.query(
            vector=vector,
            top_k=top_k,
            include_metadata=include_metadata,
            filter=filter  # puede ser None
        )

        return res

    except Exception as e:
        print(f"❌ Error en query: {e}")
        return {"matches": []}
