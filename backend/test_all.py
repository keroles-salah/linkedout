import sys, io, requests, json, re, urllib3
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
urllib3.disable_warnings()

BASE = "http://localhost:5078"
s = requests.Session()

def test(method, url, label, auth=False, expected_status=None, json_data=None, check_text=None):
    full_url = BASE + url
    try:
        if method == "GET":
            r = s.get(full_url, verify=False, allow_redirects=False, timeout=10)
        elif method == "POST":
            r = s.post(full_url, json=json_data, verify=False, allow_redirects=False, timeout=10)
        elif method == "PUT":
            r = s.put(full_url, json=json_data, verify=False, allow_redirects=False, timeout=10)
        elif method == "DELETE":
            r = s.delete(full_url, verify=False, allow_redirects=False, timeout=10)
        else:
            r = s.request(method, full_url, verify=False, allow_redirects=False, timeout=10)
        
        status = r.status_code
        symbol = "✅" if (expected_status and status == expected_status) or (not expected_status and 200 <= status < 400) else "⚠️" if status == 302 else "❌"
        detail = ""
        if check_text and r.text and check_text in r.text:
            detail += f" [contains '{check_text}']"
        if 400 <= status < 500 and not expected_status:
            detail += f" [{r.text[:100]}]"
        
        print(f"  {symbol} {method:6} {url:45} → {status} {detail}")
        return r
    except Exception as e:
        print(f"  ❌ {method:6} {url:45} → ERR: {str(e)[:80]}")
        return None

print("=" * 80)
print("LINKDOUT — Comprehensive Endpoint & Method Analysis")
print("=" * 80)

# ============================
# PHASE 1: PUBLIC ENDPOINTS
# ============================
print("\n📁 PHASE 1: Public Pages & Auth Endpoints")
print("-" * 80)

test("GET", "/", "Home (Landing)")
test("GET", "/Account/Login", "Login Page", check_text="تسجيل")
test("GET", "/Account/Register", "Register Page")
test("GET", "/Account/Logout", "Logout")
test("POST", "/Auth/Login", "Auth Login POST", json_data={"email":"test@test.com","password":"test"})
test("POST", "/Auth/Register", "Auth Register POST", json_data={"email":"x@y.com","fullName":"T","password":"123456"})

# Static assets
test("GET", "/css/linkdout.css", "CSS file", expected_status=200)

# ============================
# PHASE 2: LOGIN
# ============================
print("\n📁 PHASE 2: Authenticate & Test Protected Routes")
print("-" * 80)

# Try to login with a known user
# First get the login page to capture the anti-forgery token
login_r = s.get(BASE + "/Account/Login", verify=False, timeout=10)
token_match = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', login_r.text)
token = token_match.group(1) if token_match else ""

# Try auth API login
r = test("POST", "/api/auth/login", "API Login (test)", json_data={"email":"keroles@linkdout.com","password":"123"})

# Check if logged in
home_r = test("GET", "/", "Home (authenticated)")

# Try with a known user from DB
if not token_match:
    # Try direct cookie auth 
    pass

# ============================
# PHASE 3: ALL API ENDPOINTS
# ============================
print("\n📁 PHASE 3: API Endpoints")
print("-" * 80)

apis = [
    # Notifications
    ("GET", "/api/notifications", "Notifications list"),
    ("GET", "/api/notifications/count", "Notification count"),
    # Interactions
    ("GET", "/api/interactions/like/1", "Check like status"),
    ("POST", "/api/interactions/like/1", "Like post 1"),
    ("GET", "/api/interactions/comments/1", "Get comments post 1"),
    ("POST", "/api/interactions/comment/1", "Comment on post 1", {"body": "Test!"}),
    ("POST", "/api/interactions/react/1", "React to post 1", {"type": "like"}),
    ("POST", "/api/interactions/share/1", "Share post 1"),
    ("POST", "/api/interactions/view/1", "View post 1"),
    ("POST", "/api/interactions/bookmark/1", "Bookmark post 1"),
    ("GET", "/api/interactions/bookmarks", "Get bookmarks"),
    # Users
    ("GET", "/api/users/1", "User by ID"),
    ("GET", "/api/users/search?q=ker", "Search users"),
    # Search
    ("GET", "/api/search/autocomplete?q=ke", "Search autocomplete"),
    # Groups
    ("POST", "/api/interactions/groups/join/1", "Join group 1"),
    # Connect
    ("POST", "/api/interactions/connect/2", "Connect user 2"),
    # Posts
    ("POST", "/api/interactions/posts", "Create post", {"body": "API test post", "tags": ["test"]}),
]

for method, url, label, *rest in apis:
    jd = rest[0] if rest else None
    test(method, url, label, json_data=jd)

# ============================
# PHASE 4: ALL PAGES
# ============================
print("\n📁 PHASE 4: All Pages (View Rendering)")
print("-" * 80)

pages = [
    ("/", "Home / Feed"),
    ("/Leaderboard", "Leaderboard"),
    ("/Jobs/Match", "Job Match"),
    ("/Profile", "My Profile"),
    ("/Profile/edit", "Edit Profile"),
    ("/Search", "Search"),
    ("/Search?q=python", "Search (query)"),
    ("/Connections", "Connections"),
    ("/Circles", "Circles"),
    ("/Groups", "Groups"),
    ("/Groups/1", "Group Detail"),
    ("/Companies", "Companies"),
    ("/Companies/1", "Company Detail"),
    ("/Opportunities", "Opportunities"),
    ("/Notifications", "Notifications"),
    ("/Posts/1", "Post Detail"),
]

for url, label in pages:
    r = test("GET", url, label)
    if r and r.status_code == 200:
        # Quick checks
        has_layout = "linkdout.css" in r.text
        has_glass = "glass-card" in r.text or "glass" in r.text.lower() or "backdrop-filter" in r.text
        has_rtl = 'dir="rtl"' in r.text or 'direction:rtl' in r.text.replace(' ', '') or 'lang="ar"' in r.text
        issues = []
        if not has_layout: issues.append("No CSS ref")
        if not has_rtl: issues.append("No RTL")
        if issues:
            print(f"     ⚠️ Issues: {', '.join(issues)}")

print("\n" + "=" * 80)
print("ANALYSIS COMPLETE")
print("=" * 80)
