import sys, os, re
sys.stdout.reconfigure(encoding='utf-8')
p = r'C:\Users\ke_ro\.openclaw-autoclaw\workspace\linkdout\backend\Linkdout.Api\Data\SeedData.cs'
with open(p, 'r', encoding='utf-8') as f:
    content = f.read()

# Find User creation pattern and add XP/Badges after ProfileViews/MemberCount etc
# Look for a pattern like: new User { ... CreatedAt = ... }
# Add XP and Badges to each user

# Strategy: find each "new User" block and add XP/Badges after CreatedAt
users_xp = [
    (1, "كيرلس صلاح فخري", 350, '["🖊️ أول منشور","✍️ كاتب محتوى","❤️ محبوب","💬 متفاعل","🔗 شبكة قوية","⚡ نشيط"]'),
    (2, "سارة أحمد", 220, '["🖊️ أول منشور","❤️ محبوب","💬 متفاعل","⚡ نشيط"]'),
    (3, "محمد علي", 180, '["🖊️ أول منشور","❤️ محبوب","⚡ نشيط"]'),
    (4, "نورا حسن", 120, '["🖊️ أول منشور","💬 متفاعل"]'),
    (5, "أحمد محمود", 90, '["🖊️ أول منشور"]'),
    (6, "ليلى سامي", 450, '["🖊️ أول منشور","✍️ كاتب محتوى","❤️ محبوب","💬 متفاعل","🔗 شبكة قوية","⚡ نشيط","🔥 ملتهب","👁️ ملف مميز"]'),
    (7, "عمر خالد", 45, '["🖊️ أول منشور"]'),
    (8, "فاطمة نبيل", 300, '["🖊️ أول منشور","✍️ كاتب محتوى","❤️ محبوب","💬 متفاعل","🔗 شبكة قوية","⚡ نشيط"]'),
]

# For each user, find their creation block and add XP/Badges  
for uid, name, xp, badges in users_xp:
    # Find the user block - look for FullName containing the name
    pattern = rf'(new User\s*\{{[^}}]*?FullName\s*=\s*"{re.escape(name)}"[^}}]*?\}})(\s*,)'
    # This regex approach is fragile. Instead, find by matching on each user
    # Let's use a simpler approach: find FullName line and insert after ProfileViews
    
    name_pattern = f'FullName = "{name}"'
    idx = content.find(name_pattern)
    if idx == -1:
        print(f'NOT FOUND: {name}')
        continue
    
    # Find the end of this user's block (next "new User" or end of users list)
    next_user = content.find('new User', idx + 1)
    if next_user == -1:
        next_user = content.find('// --- P', idx + 1)  # look for end of user section
    
    block_end = content.rfind('}', idx, next_user)
    if block_end == -1:
        block_end = content.find('}', idx)
    
    print(f'{name}: idx={idx}, block ends at {block_end}')
    
    # Find CreatedAt or ProfileViews near the block end  
    created_at = content.rfind('CreatedAt', idx, block_end)
    if created_at > 0:
        # Find the end of that line
        line_end = content.find('\n', created_at)
        # Insert XP and Badges after that line
        insert_text = f'\n                        XP = {xp}, Badges = {badges},'
        before = content[:line_end]
        after = content[line_end:]
        content = before + insert_text + after
        print(f'  Added XP={xp}, Badges to {name}')

with open(p, 'w', encoding='utf-8') as f:
    f.write(content)
print('\nDone updating seed data')
