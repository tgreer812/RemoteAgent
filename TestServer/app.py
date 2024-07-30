from flask import Flask, jsonify
import threading
import time
import json

app = Flask(__name__)

@app.route('/api/tasking', methods=['GET'])
def get_tasking():
    # Simulate a tasking response
    example_task_path = '.\\TestTasking\\DirectoryListTask.json'
    with open(example_task_path, 'r') as task_file:
        task = json.load(task_file)

    return jsonify(task)

def run_flask_app():
    app.run(debug=True, use_reloader=False)

if __name__ == '__main__':
    # Run the Flask app in a separate thread to allow for non-blocking behavior
    flask_thread = threading.Thread(target=run_flask_app)
    flask_thread.start()
    
    print("Flask app is running and ready to be polled at http://localhost:5000/api/tasking")