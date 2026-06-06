import sys, urllib.request, urllib.parse, http.cookiejar, re
sys.stdout.reconfigure(encoding='utf-8')

cj = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj))

# Get login page for anti-forgery token
r = opener.open('http://localhost:5078/Account/Login')
body = r.read().decode()
token_match = re.search(r'__RequestVerificationToken[^>]+value="([^"]+)"', body)
token = token_match.group(1) if token_match else None
print(f'CSRF token: {token[:20] if token else "NONE"}...')

# POST login
data = urllib.parse.urlencode({
    '__RequestVerificationToken': token,
    'Email': 'keroles@linkdout.com',
    'Password': 'Test1234'
}).encode()
r = opener.open(urllib.request.Request('http://localhost:5078/Account/Login', data))
print(f'POST Login -> {r.status} | url: {r.url} | len: {len(r.read())}')

# Test Leaderboard
r = opener.open('http://localhost:5078/Leaderboard')
body = r.read().decode()
print(f'\nLeaderboard: {r.status} | {len(body)} chars')

m = re.search(r'<main[^>]*>(.*?)</main>', body, re.DOTALL)
if m:
    content = m.group(1)
    print(f'Main (first 400): {content[:400]}')
    if '🥇' in content: print('✅ Has gold medal')
    if 'XP' in content: print('✅ Has XP')
else:
    if 'تسجيل الدخول' in body[:500]:
        print('⚠️ Redirected to login')
    else:
        print(f'No main - body start: {body[:300]}')

# Test JobMatch
r = opener.open('http://localhost:5078/Jobs/Match')
body = r.read().decode()
print(f'\nJobMatch: {r.status} | {len(body)} chars')
m = re.search(r'<main[^>]*>(.*?)</main>', body, re.DOTALL)
if m:
    content = m.group(1)
    print(f'Main (first 400): {content[:400]}')
    if 'تطابق' in content: print('✅ Has matching content')
else:
    print(f'No main - body start: {body[:300]}')
