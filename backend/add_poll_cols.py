import pymysql

COLUMNS = [
    ('Poll', "LONGTEXT NULL"),
    ('IsEdited', "TINYINT(1) NOT NULL DEFAULT 0"),
]

try:
    conn = pymysql.connect(host='localhost', user='root', password='', database='linkdout')
    cursor = conn.cursor()
    for col, ctype in COLUMNS:
        cursor.execute(f"SHOW COLUMNS FROM Posts LIKE '{col}'")
        if cursor.fetchone():
            print(f"{col} already exists")
        else:
            cursor.execute(f"ALTER TABLE Posts ADD COLUMN {col} {ctype}")
            conn.commit()
            print(f"{col} added")
    cursor.close()
    conn.close()
except Exception as e:
    print(f"Error: {e}")
