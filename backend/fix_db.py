import sys
sys.path.insert(0, r'C:\Users\ke_ro\pip')

try:
    import mysql.connector
except:
    print("mysql.connector not found, trying pymysql...")
    import pymysql as mysql

try:
    conn = mysql.connect(
        host='localhost',
        user='root',
        password='',
        database='linkdout'
    )
    cursor = conn.cursor()
    
    # Check if ViewCount column exists
    cursor.execute("SHOW COLUMNS FROM Posts LIKE 'ViewCount'")
    result = cursor.fetchone()
    
    if result:
        print(f"ViewCount column already exists: {result}")
    else:
        cursor.execute("ALTER TABLE Posts ADD COLUMN ViewCount INT NOT NULL DEFAULT 0")
        conn.commit()
        print("ViewCount column added successfully")
    
    # Verify
    cursor.execute("SHOW COLUMNS FROM Posts")
    columns = cursor.fetchall()
    print("\nPosts table columns:")
    for col in columns:
        print(f"  {col[0]} ({col[1]})")
    
    cursor.close()
    conn.close()
except Exception as e:
    print(f"Error: {e}")
