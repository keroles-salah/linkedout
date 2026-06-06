import requests, re, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
s = requests.Session()

# Login
r = s.get('http://localhost:5078/Account/Login', verify=False)
token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', r.text)
s.post('http://localhost:5078/Account/Login', 
       data={'Email':'keroles@linkdout.com','Password':'admin123',
             '__RequestVerificationToken':token.group(1) if token else ''}, 
       allow_redirects=False, verify=False)

# Test home page
r = s.get('http://localhost:5078/', verify=False, allow_redirects=False)
print(f'Home: {r.status_code} ({len(r.text)} bytes)')
checks = {
    'New post design (post-header)': 'post-header' in r.text,
    'Post avatar class': 'post-avatar' in r.text,
    'Post gallery grid': 'post-gallery' in r.text,
    'Post stats row': 'post-stats' in r.text,
    'Action labels': 'action-label' in r.text,
    'imgbb upload button': 'imageUpload' in r.text,
    'Upload preview area': 'upload-preview' in r.text,
    'No old composer URL input': 'composer-images' not in r.text,
    'Verified badge': 'verified' in r.text,
}
for k, v in checks.items():
    print(f"  {'[OK]' if v else '[FAIL]'} {k}")

# Check for errors
if 'Exception' in r.text or 'error CS' in r.text:
    print('  WARNING: Page has compile errors!')
    idx = r.text.find('Exception')
    if idx > 0: print(f'    {r.text[idx:idx+200]}')
else:
    print('  No compile errors')
