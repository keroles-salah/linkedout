import pymysql

INDEXES = [
    # Posts: speed up feed queries
    ("idx_posts_user_created", "Posts", "UserId, CreatedAt DESC"),
    ("idx_posts_group", "Posts", "GroupId"),
    ("idx_posts_created", "Posts", "CreatedAt DESC"),
    # Connections: speed up friend lookups
    ("idx_connections_requester_status", "Connections", "RequesterId, Status"),
    ("idx_connections_recipient_status", "Connections", "RecipientId, Status"),
    # Likes: speed up like lookups
    ("idx_likes_post_user", "Likes", "PostId, UserId"),
    # Comments
    ("idx_comments_post", "Comments", "PostId"),
    # XpTransactions
    ("idx_xp_user_created", "XpTransactions", "UserId, CreatedAt DESC"),
    # Jobs
    ("idx_jobs_active", "Jobs", "IsActive"),
    # Users
    ("idx_users_profileviews", "Users", "ProfileViews DESC"),
    # PostBookmarks
    ("idx_bookmarks_user", "PostBookmarks", "UserId"),
]

try:
    conn = pymysql.connect(host='localhost', user='root', password='', database='linkdout')
    cursor = conn.cursor()
    
    # Get existing indexes
    cursor.execute("""
        SELECT INDEX_NAME, TABLE_NAME FROM information_schema.STATISTICS 
        WHERE TABLE_SCHEMA = 'linkdout' AND INDEX_NAME LIKE 'idx_%'
        GROUP BY INDEX_NAME, TABLE_NAME
    """)
    existing = set((row[0], row[1]) for row in cursor.fetchall())
    
    added, skipped = 0, 0
    for name, table, cols in INDEXES:
        if (name, table) in existing:
            skipped += 1
            continue
        try:
            cursor.execute(f"CREATE INDEX {name} ON {table} ({cols})")
            conn.commit()
            added += 1
            print(f"[ADDED] {name} ON {table}({cols})")
        except Exception as e:
            print(f"[ERROR] {name}: {e}")
    
    print(f"\nResults: {added} added, {skipped} already existed, {len(INDEXES)-added-skipped} errors")
    cursor.close()
    conn.close()
except Exception as e:
    print(f"Connection error: {e}")
