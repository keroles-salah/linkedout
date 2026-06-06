import sys
sys.path.insert(0, r'C:\Users\ke_ro\.openclaw-autoclaw\skills\gptapi\scripts')
from chatgpt_api import enqueue

task_id = enqueue(
    prompt="You are a UI/UX expert. Analyze this image of Arabic social network Linkdout. Describe layout, broken elements, formatting errors, empty stats, and list 5-10 fixes by priority. Be specific about exact elements and text.",
    image_path=r"C:\Users\ke_ro\.openclaw-autoclaw\workspace\.autoclaw-attachments\20260605-212335-981e5f26-76f-clipboard-1780683775256.png"
)
print(f"TASK_ID={task_id}")
print("Run: python chatgpt_worker.py from skills/gptapi/scripts/")
