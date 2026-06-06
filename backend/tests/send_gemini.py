import sys
sys.path.insert(0, r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\telegram_replier\temp')
from gemini_api_v2 import enqueue

task_id = enqueue(
    prompt='''Analyze this screenshot of an Arabic social networking website called "Linkdout" in extreme detail.

Describe EVERYTHING you see:
1. Layout structure (columns, sections, spacing)
2. Colors and design quality
3. All text content visible (even Arabic)
4. Any UI/UX issues: misaligned elements, broken layouts, empty sections, poor spacing, visual bugs
5. Any content issues: empty stats, wrong language (English mixed with Arabic), text overflow, truncation
6. Quality of the overall design - does it look professional? What needs improvement?

Be brutally honest and thorough.''',
    image_path=r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\.autoclaw-attachments\20260605-212335-981e5f26-76f-clipboard-1780683775256.png',
    new_chat=True
)
print(f'TASK_ID={task_id}')
