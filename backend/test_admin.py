import requests, sys, io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

s = requests.Session()
BASE = "http://localhost:5078"

# Get login page for anti-forgery token
r = s.get(f"{BASE}/Account/Login", verify=False)
import re
token = re.search(r'name="__RequestVerificationToken"[^>]*value="([^"]+)"', r.text)

# Get users from DB to find correct credentials
import pymysql
conn = pymysql.connect(host='localhost', user='root', password='', database='linkdout')
cursor = conn.cursor()
cursor.execute("SELECT Email, Role FROM Users WHERE Role='admin' LIMIT 1")
admin = cursor.fetchone()
cursor.close()
conn.close()

print(f"Admin user: {admin}")

# Login as admin
login_data = {
    "Email": admin[0],
    "Password": "admin123",
}
if token:
    login_data["__RequestVerificationToken"] = token.group(1)

r = s.post(f"{BASE}/Account/Login", data=login_data, allow_redirects=False, verify=False)
print(f"Login: {r.status_code} → {r.headers.get('Location','no redirect')}")

# Test Admin pages
for path, label in [
    ("/Admin", "Dashboard"),
    ("/Admin/Users", "Users"),
    ("/Admin/Posts", "Posts"), 
    ("/Admin/Analytics", "Analytics"),
]:
    r = s.get(f"{BASE}{path}", allow_redirects=False, verify=False)
    has_admin = "لوحة التحكم" in r.text or "الإحصائيات" in r.text or "إدارة" in r.text
    has_sidebar = "admin-sidebar" in r.text or "admin-nav" in r.text
    print(f"  {label}: {r.status_code} ({len(r.text)} bytes) | admin_ui={has_admin} | sidebar={has_sidebar}")

# Test APIs
for path in ["/api/admin/stats", "/api/admin/charts"]:
    r = s.get(f"{BASE}{path}", verify=False)
    try:
        data = r.json()
        if "totalUsers" in data:
            print(f"  {path}: OK — users={data['totalUsers']} posts={data['totalPosts']}")
        elif "growth" in data:
            print(f"  {path}: OK — growth points={len(data['growth']['data'])}")
        else:
            print(f"  {path}: {r.status_code} — {data}")
    except:
        print(f"  {path}: {r.status_code} — {r.text[:100]}")
