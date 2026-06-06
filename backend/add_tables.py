import pymysql
c = pymysql.connect(host='localhost', user='root', password='', database='linkdout').cursor()

# Profile view tracking table
c.execute("SHOW TABLES LIKE 'ProfileViews'")
if not c.fetchone():
    c.execute("""CREATE TABLE ProfileViews (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        ViewerId INT NOT NULL,
        ProfileId INT NOT NULL,
        CreatedAt DATETIME NOT NULL DEFAULT NOW(),
        INDEX idx_profileviews_profile (ProfileId, CreatedAt DESC),
        INDEX idx_profileviews_viewer (ViewerId)
    )""")
    c.connection.commit()
    print('ProfileViews table created')
else: print('ProfileViews exists')

# User stories table
c.execute("SHOW TABLES LIKE 'UserStories'")
if not c.fetchone():
    c.execute("""CREATE TABLE UserStories (
        Id INT AUTO_INCREMENT PRIMARY KEY,
        UserId INT NOT NULL,
        Body VARCHAR(2000) NULL,
        ImageUrl VARCHAR(1000) NULL,
        Color VARCHAR(50) NULL DEFAULT '#7C3AED',
        CreatedAt DATETIME NOT NULL DEFAULT NOW(),
        ExpiresAt DATETIME NOT NULL,
        INDEX idx_stories_user (UserId, ExpiresAt DESC)
    )""")
    c.connection.commit()
    print('UserStories table created')
else: print('UserStories exists')

c.close()
print('Done')
