import sqlite3

conn = sqlite3.connect('miku.db')
c = conn.cursor()

def create_guilds_table():
    c.execute("CREATE TABLE IF NOT EXISTS Guilds (id VARCHAR(100) PRIMARY KEY, name VARCHAR(100), prefix VARCHAR(2));")

def create_bank_table():
    c.execute("CREATE TABLE IF NOT EXISTS MikuBank (id VARCHAR(100) PRIMARY KEY, balance INT);")

def create_users_table():
    c

create_guilds_table()
create_bank_table()

conn.commit()