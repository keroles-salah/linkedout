import pymysql

try:
    conn = pymysql.connect(host='localhost', user='root', password='', database='linkdout')
    cursor = conn.cursor()
    
    for col, coltype in [('Role', "VARCHAR(50) NOT NULL DEFAULT 'user'"), ('IsActive', 'TINYINT(1) NOT NULL DEFAULT 1')]:
        cursor.execute(f"SHOW COLUMNS FROM Users LIKE '{col}'")
        if cursor.fetchone():
            print(f"{col} already exists")
        else:
            cursor.execute(f"ALTER TABLE Users ADD COLUMN {col} {coltype}")
            conn.commit()
            print(f"{col} added")
    
    # Make first user an admin
    cursor.execute("UPDATE Users SET Role = 'admin' WHERE Id = (SELECT MIN(Id) FROM (SELECT Id FROM Users) AS u)")
    conn.commit()
    print("First user promoted to admin")
    
    # Verify
    cursor.execute("SELECT Id, FullName, Role, IsActive FROM Users LIMIT 5")
    for row in cursor.fetchall():
        print(f"  User {row[0]}: {row[1]} | Role={row[2]} | Active={row[3]}")
    
    cursor.close()
    conn.close()
except Exception as e:
    print(f"Error: {e}")
