import re, os

base = r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\linkdout\backend\Linkdout.Api'
layout_path = os.path.join(base, 'Views', 'Shared', '_layout.cshtml')
css_dir = os.path.join(base, 'wwwroot', 'css')
css_path = os.path.join(css_dir, 'linkdout.css')

os.makedirs(css_dir, exist_ok=True)

with open(layout_path, 'r', encoding='utf-8') as f:
    content = f.read()

# Find the <style> block (there's only one main block after fonts)
match = re.search(r'<style>\s*\n(.*?)</style>', content, re.DOTALL)
if not match:
    print('ERROR: Could not find style block')
    exit(1)

css = match.group(1).rstrip()

# Write CSS file
with open(css_path, 'w', encoding='utf-8') as f:
    f.write('/* Linkdout Shared Styles */\n')
    f.write(css)
    f.write('\n')

print(f'Extracted {len(css)} bytes to {css_path}')

# Replace style block in layout with link tag
link_tag = '<link rel="stylesheet" href="/css/linkdout.css">'
new_content = re.sub(
    r'<style>\s*\n.*?</style>',
    link_tag,
    content,
    count=1,
    flags=re.DOTALL
)

with open(layout_path, 'w', encoding='utf-8') as f:
    f.write(new_content)

print(f'Updated layout: {len(new_content)} bytes (was {len(content)})')
print('Done!')
