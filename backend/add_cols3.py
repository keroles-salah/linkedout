import pymysql
c = pymysql.connect(host='localhost', user='root', password='', database='linkdout').cursor()
for col, ctype, table in [
    ('SaveCount', 'INT NOT NULL DEFAULT 0', 'Posts'),
    ('LastActiveAt', 'DATETIME NULL', 'Users'),
]:
    c.execute(f"SHOW COLUMNS FROM {table} LIKE '{col}'")
    if not c.fetchone():
        c.execute(f"ALTER TABLE {table} ADD COLUMN {col} {ctype}")
        c.connection.commit()
        print(f'{col} added to {table}')
    else: print(f'{col} exists')
# Add reaction types tracking
c.execute("SHOW COLUMNS FROM Likes LIKE 'ReactionType'")
if not c.fetchone():
    c.execute("ALTER TABLE Likes ADD COLUMN ReactionType VARCHAR(50) NULL DEFAULT 'like'")
    c.connection.commit()
    print('ReactionType added to Likes')
else: print('ReactionType exists')
c.close()
