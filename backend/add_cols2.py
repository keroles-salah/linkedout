import pymysql
conn = pymysql.connect(host='localhost', user='root', password='', database='linkdout')
c = conn.cursor()
for col, ctype, table in [
    ('ParentCommentId', 'INT NULL', 'Comments'),
    ('CoverColor', "VARCHAR(50) NULL DEFAULT '#7C3AED'", 'Users'),
]:
    c.execute(f"SHOW COLUMNS FROM {table} LIKE '{col}'")
    if not c.fetchone():
        c.execute(f"ALTER TABLE {table} ADD COLUMN {col} {ctype}")
        conn.commit()
        print(f'{col} added to {table}')
    else: print(f'{col} exists')
c.close(); conn.close()
