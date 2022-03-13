from sys import stderr
from flask import Flask, request

import db
import sqlite3 as sqlite

app = Flask(__name__)

@app.route("/mikuapi")
def ping():
    return "miku data api v1.0"

@app.route("/mikuapi/addguild", methods=[ 'POST' ])
def add_guild():
    guild = request.json
    conn = sqlite.connect("miku.db")
    c = conn.cursor()
    params = (guild["id"], guild["name"], "!")
    c.execute("INSERT INTO Guilds Values(?, ?, ?)", params)
    conn.commit()

    return "guild added"

@app.route("/mikuapi/getprefix/<guild>", methods = [ 'GET' ])
def get_prefix(guild):
    conn = sqlite.connect("miku.db")
    c = conn.cursor()
    prefix = c.execute("SELECT prefix FROM Guilds WHERE id = ?;", (guild,)).fetchone()[0]

    return prefix

@app.route("/mikuapi/changeprefix/<guild>", methods = [ 'GET', 'POST' ])
def change_prefix(guild):
    print(request.get_json(), file=stderr)
    json = request.json
    conn = sqlite.connect("miku.db")
    c = conn.cursor()
    c.execute("UPDATE Guilds SET prefix = ? WHERE id = ?;", (json["prefix"], guild))
    conn.commit()

    return f"prefix for guild {guild} updated to {json['prefix']}"

@app.route("/mikuapi/addusers", methods=[ 'POST' ])
def add_users():
    json = request.json
    print(json, file=stderr)

    return ""

@app.route("/mikuapi/bank/createaccount/<id>", methods=[ 'POST' ])
def create_account(id):
    conn = sqlite.connect("miku.db")
    c = conn.cursor()
    c.execute("INSERT INTO MikuBank VALUES(?, 50)",(id,))
    conn.commit()

    return f"account with id {id} created"

@app.route("/mikuapi/bank/getbalance/<id>")
def get_balance(id):
    ret = ""
    conn = sqlite.connect("miku.db")
    c = conn.cursor()
    try:
        balance = c.execute("SELECT balance FROM MikuBank WHERE id = ?", (id,)).fetchone()[0]
        ret = str(balance)
    except Exception as e:
        if(type(e).__name__ == 'TypeError'):
            ret = "no account"

    return ret

@app.route("/mikuapi/bank/addfunds", methods=[ 'POST' ])
def add_funds():
    ret = ""
    json = request.json
    conn = sqlite.connect("miku.db")
    c = conn.cursor()
    c.execute("UPDATE MikuBank SET balance = balance + ? WHERE id = ?", (json["amount"], json["recipientId"]))

    return f"{json['amount']} added to account {json['recipientId']}"
    
@app.route("/mikuapi/bank/subtractfunds", methods=[ 'POST' ])
def subtract_funds():
    ret = ""
    json = request.json
    conn = sqlite.connect("miku.db")
    c = conn.cursor()
    c.execute("UPDATE MikuBank SET balance = balance - ? WHERE id = ?", (json["amount"], json["recipientId"]))

    return f"{json['amount']} taken from account {json['recipientId']}"

@app.route("/mikuapi/bank/transfer")
def transfer():
    json = request.json
    conn = sqlite.connect("miku.db")
    c = conn.cursor()
    params = (json["amount"], json["senderId"])
    c.execute("UPDATE MikuBank SET balance = balance - ? WHERE id = ?", params)
    params = (json["amount"], json["recipientId"])
    c.execute("UPDATE MikuBank SET balance = balance + ? WHERE id = ?", params)
    conn.commit()

    return "transfer complete"

app.run(host="127.0.0.1", port=5000, threaded=True)