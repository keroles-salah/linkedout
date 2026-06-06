import urllib.request, urllib.parse, http.cookiejar, json, sys
sys.stdout.reconfigure(encoding='utf-8')
cj = http.cookiejar.CookieJar()
opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj), urllib.request.HTTPRedirectHandler())

# Login
data = urllib.parse.urlencode({'Email':'keroles@linkdout.com','Password':'Test1234'}).encode()
req = urllib.request.Request('http://localhost:5078/Account/Login', data=data, method='POST')
req.add_header('Content-Type','application/x-www-form-urlencoded')
opener.open(req)
print('Logged in!')

# Test like
r = opener.open('http://localhost:5078/api/interactions/like/1', data=b'')
d = json.loads(r.read().decode('utf-8','ignore'))
print(f'Like: {r.status} -> liked={d.get("liked")}, count={d.get("likeCount")}')

# Test unlike
r = opener.open('http://localhost:5078/api/interactions/like/1', data=b'')
d = json.loads(r.read().decode('utf-8','ignore'))
print(f'Unlike: {r.status} -> liked={d.get("liked")}, count={d.get("likeCount")}')

# Test share
r = opener.open('http://localhost:5078/api/interactions/share/1', data=b'')
d = json.loads(r.read().decode('utf-8','ignore'))
print(f'Share: {r.status} -> count={d.get("shareCount")}')

# Test connect
r = opener.open('http://localhost:5078/api/interactions/connect/7', data=b'')
d = json.loads(r.read().decode('utf-8','ignore'))
print(f'Connect: {r.status} -> status={d.get("status")}')

# Create post
post_data = json.dumps({'body': 'First post from Linkdout!', 'tags': ['launch','AI']}).encode()
req = urllib.request.Request('http://localhost:5078/api/interactions/posts', data=post_data, method='POST')
req.add_header('Content-Type','application/json')
r = opener.open(req)
d = json.loads(r.read().decode('utf-8','ignore'))
print(f'Create Post: {r.status} -> ID={d.get("id")}')

# Add comment
comment_data = json.dumps({'body': 'Great post!'}).encode()
req = urllib.request.Request('http://localhost:5078/api/interactions/comment/1', data=comment_data, method='POST')
req.add_header('Content-Type','application/json')
r = opener.open(req)
d = json.loads(r.read().decode('utf-8','ignore'))
print(f'Comment: {r.status} -> author={d.get("Author")}')

# Join group
r = opener.open('http://localhost:5078/api/interactions/groups/join/1', data=b'')
d = json.loads(r.read().decode('utf-8','ignore'))
print(f'Join Group: {r.status} -> isMember={d.get("isMember")}')

print('\nAll interactions tested!')
