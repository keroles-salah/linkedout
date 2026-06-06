import sys
sys.path.insert(0, r'C:\Users\ke_ro\.openclaw-autoclaw\skills\gptapi\scripts')
from chatgpt_api import enqueue

task_id = enqueue(
    prompt="""You are a senior UI/UX auditor. Analyze this screenshot of an Arabic social networking website "Linkdout" (Groups page - المجموعات).

Analyze thoroughly:
1. Overall layout quality and visual balance
2. Grid/card alignment - are cards properly sized and aligned?
3. Typography - sizes, readability, consistency across cards
4. Spacing - margins, padding, gaps between elements
5. Colors - any contrast or harmony issues
6. Content - any truncated text, missing data, broken elements
7. Navigation - active states visible and correct?
8. Any empty states or wasted space
9. List 5-10 concrete fixes ordered by priority
10. Overall score out of 10

Be brutally specific. Mention exact elements and text you see. This is a warm-terracotta/gold/cream themed Arabic RTL site.""",
    image_path=r"C:\Users\ke_ro\.openclaw-autoclaw\workspace\.autoclaw-attachments\20260605-214531-0a5fbb3f-f07-clipboard-1780685110702.png"
)
print(f"TASK_ID={task_id}")
print("Run: python chatgpt_worker.py from skills/gptapi/scripts/")
