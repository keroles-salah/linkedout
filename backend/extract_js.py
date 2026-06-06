import re

with open(r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\linkdout\backend\Linkdout.Api\Views\Shared\_layout.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

scripts = re.findall(r'<script[^>]*>(.*?)</script>', content, re.DOTALL)

# The last big script block
js = scripts[-1]

# Replace Razor with valid JS placeholders
js = re.sub(r'@RenderSection\(".*?",\s*required:\s*false\)', '', js)
js = re.sub(r'@\(ViewBag\.\w+ \?\? \d+\)', '1', js) 
js = re.sub(r'@\(ViewBag\.HasMore\s*!=\s*null\s*&&\s*\(bool\)ViewBag\.HasMore\s*\?\s*"(?:true|false)"\s*:\s*"(?:true|false)"\)', '"true"', js)

with open(r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\linkdout\backend\temp_js.js', 'w', encoding='utf-8') as f:
    f.write(js)

print("Written temp_js.js")
