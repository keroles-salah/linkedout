import pymysql, bcrypt

conn = pymysql.connect(host='localhost', user='root', password='', database='linkdout')
cursor = conn.cursor()

# Check admin user
cursor.execute("SELECT Id, FullName, Email, PasswordHash, Role FROM Users WHERE Role='admin'")
for row in cursor.fetchall():
    name = row[1].encode('ascii', 'replace').decode()
    email = row[2]
    print(f"User {row[0]}: {name} | {email} | Role={row[4]}")
    print(f"  Password hash length: {len(row[3])}")
    # Set password to "admin123" for testing
    import bcrypt
    new_hash = bcrypt.hashpw(b"admin123", bcrypt.gensalt()).decode()
    cursor.execute("UPDATE Users SET PasswordHash = %s WHERE Id = %s", (new_hash, row[0]))
    conn.commit()
    print(f"  Password RESET to: admin123")

# Check if any user exists with password 123
cursor.execute("SELECT Id, FullName, Email FROM Users")
users = cursor.fetchall()
print(f"\nTotal users: {len(users)}")
for u in users[:5]:
    print(f"  {u[0]}: {u[1]} ({u[2]})")

cursor.close()
conn.close()
