import re

# Read _layout.cshtml
with open(r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\linkdout\backend\Linkdout.Api\Views\Shared\_layout.cshtml', 'r', encoding='utf-8') as f:
    content = f.read()

# Extract the big script block (not the head styles/scripts)
scripts = re.findall(r'<script[^>]*>(.*?)</script>', content, re.DOTALL)

# The big inline script is the last one
big_script = scripts[-1]

# Replace Razor expressions
big_script = re.sub(r'@RenderSection\(".*?",\s*required:\s*false\)', '', big_script)
big_script = re.sub(r'@\(ViewBag\.\w+ \?\? \d+\)', '1', big_script)
big_script = re.sub(r'@\(ViewBag\.HasMore\s*!=\s*null\s*&&\s*\(bool\)ViewBag\.HasMore\s*\?\s*"(?:true|false)"\s*:\s*"(?:true|false)"\)', '"true"', big_script)

# Count brackets
open_paren = 0
close_paren = 0
open_brace = 0
close_brace = 0
open_bracket = 0
close_bracket = 0

lines = big_script.split('\n')
for i, line in enumerate(lines, 1):
    line_brace = line.count('{') - line.count('}')
    line_paren = line.count('(') - line.count(')')
    line_bracket = line.count('[') - line.count(']')
    
    open_brace += line.count('{')
    close_brace += line.count('}')
    open_paren += line.count('(')
    close_paren += line.count(')')
    open_bracket += line.count('[')
    close_bracket += line.count(']')
    
    # Track running totals
    cb = open_brace - close_brace
    cp = open_paren - close_paren
    cs = open_bracket - close_bracket
    
    if cb < 0 or cp < 0 or cs < 0:
        print(f"⚠️ Line {i}: Brace={cb} Paren={cp} Bracket={cs} - EXTRA CLOSING!")
        # Show surrounding context
        for j in range(max(0,i-3), min(len(lines), i+2)):
            print(f"  {j+1}: {lines[j].strip()[:120]}")
    
    # Show lines with significant bracket changes
    if abs(line_brace) > 1 or abs(line_paren) > 1 or abs(line_bracket) > 1:
        pass  # Not printing all

print(f"\n=== FINAL COUNTS ===")
print(f"Curly braces: {{={open_brace} }}={close_brace} diff={open_brace-close_brace}")
print(f"Parentheses: (={open_paren} )={close_paren} diff={open_paren-close_paren}")
print(f"Brackets: [={open_bracket} ]={close_bracket} diff={open_bracket-close_bracket}")

# Print lines where diff becomes negative first
cb = 0; cp = 0; cs = 0
for i, line in enumerate(lines, 1):
    line_stripped = line.strip()
    if not line_stripped:
        continue
    old_cb = cb; old_cp = cp; old_cs = cs
    cb += line.count('{') - line.count('}')
    cp += line.count('(') - line.count(')')
    cs += line.count('[') - line.count(']')
    if (cb < 0 and old_cb >= 0) or (cp < 0 and old_cp >= 0) or (cs < 0 and old_cs >= 0):
        print(f"\n🔴 NEGATIVE at line {i}: Brace={cb} Paren={cp} Bracket={cs}")
        for j in range(max(0,i-3), min(len(lines), i+2)):
            print(f"  {j+1}: {lines[j].strip()[:130]}")
