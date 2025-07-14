#!/usr/bin/env python3
"""
Temporary C2 Server for testing the RemoteAgent
"""
import json
from http.server import HTTPServer, BaseHTTPRequestHandler
import urllib.parse

class TempC2Handler(BaseHTTPRequestHandler):
    # Store registered agents and their jobs
    agents = {}
    pending_jobs = {}
    completed_jobs = {}
    next_agent_id = 1
    next_job_id = 1
    
    def do_POST(self):
        """Handle POST requests"""
        content_length = int(self.headers.get('Content-Length', 0))
        post_data = self.rfile.read(content_length).decode('utf-8') if content_length > 0 else ""
        
        print(f"POST {self.path}")
        if post_data:
            print(f"Data: {post_data}")
        
        if self.path == "/api/agent/hello":
            self.handle_agent_hello(post_data)
        elif self.path == "/api/job":
            self.handle_job_result(post_data)
        else:
            self.send_error(404, "Not Found")
    
    def do_GET(self):
        """Handle GET requests"""
        print(f"GET {self.path}")
        
        if self.path.startswith("/api/tasking/"):
            agent_id = self.path.split("/")[-1]
            self.handle_tasking_request(agent_id)
        elif self.path == "/api/test":
            self.handle_test_endpoint()
        elif self.path == "/api/logs":
            self.handle_logs_endpoint()
        else:
            self.send_error(404, "Not Found")
    
    def handle_agent_hello(self, post_data):
        """Handle agent registration"""
        try:
            # Assign new agent ID
            agent_id = self.next_agent_id
            self.next_agent_id += 1
            
            # Store agent info
            self.agents[agent_id] = {
                "registered_at": "now",
                "data": post_data
            }
            
            # Create a DirectoryListPlugin job for this agent
            job_id = self.next_job_id
            self.next_job_id += 1
            
            self.pending_jobs[agent_id] = [{
                "JobId": job_id,
                "JobType": "DirectoryListPlugin", 
                "JobData": {
                    "Path": "C:\\"
                }
            }]
            
            response = {"agentId": agent_id}
            
            print(f"Registered new agent with ID: {agent_id}")
            print(f"Created DirectoryList job {job_id} for agent {agent_id}")
            
            self.send_json_response(200, response)
            
        except Exception as e:
            print(f"Error in agent hello: {e}")
            self.send_error(500, str(e))
    
    def handle_tasking_request(self, agent_id):
        """Handle tasking requests from agents"""
        try:
            agent_id = int(agent_id)
            
            if agent_id not in self.agents:
                self.send_error(404, "Agent not found")
                return
            
            # Get pending jobs for this agent
            jobs = self.pending_jobs.get(agent_id, [])
            
            if jobs:
                response = {"jobs": jobs}
                print(f"Sending {len(jobs)} job(s) to agent {agent_id}")
                # Clear pending jobs after sending
                self.pending_jobs[agent_id] = []
            else:
                response = {"jobs": []}
                print(f"No pending jobs for agent {agent_id}")
            
            self.send_json_response(200, response)
            
        except Exception as e:
            print(f"Error in tasking request: {e}")
            self.send_error(500, str(e))
    
    def handle_job_result(self, post_data):
        """Handle job results from agents"""
        try:
            result_data = json.loads(post_data) if post_data else {}
            
            print("=== JOB RESULT RECEIVED ===")
            print(json.dumps(result_data, indent=2))
            print("===========================")
            
            # Store the result
            job_id = result_data.get("JobId", "unknown")
            self.completed_jobs[job_id] = result_data
            
            self.send_json_response(200, {"status": "success"})
            
        except Exception as e:
            print(f"Error in job result: {e}")
            self.send_error(500, str(e))
    
    def handle_test_endpoint(self):
        """Handle test endpoint for health checks"""
        try:
            response = {
                "status": "ok",
                "agents": len(self.agents),
                "pending_jobs": sum(len(jobs) for jobs in self.pending_jobs.values()),
                "completed_jobs": len(self.completed_jobs)
            }
            self.send_json_response(200, response)
        except Exception as e:
            print(f"Error in test endpoint: {e}")
            self.send_error(500, str(e))
    
    def handle_logs_endpoint(self):
        """Handle logs endpoint for test verification"""
        try:
            response = {
                "agents": self.agents,
                "pending_jobs": self.pending_jobs,
                "completed_jobs": self.completed_jobs
            }
            self.send_json_response(200, response)
        except Exception as e:
            print(f"Error in logs endpoint: {e}")
            self.send_error(500, str(e))

    def send_json_response(self, status_code, data):
        """Send JSON response"""
        response_json = json.dumps(data)
        self.send_response(status_code)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Content-Length', str(len(response_json)))
        self.end_headers()
        self.wfile.write(response_json.encode('utf-8'))
    
    def log_message(self, format, *args):
        """Override to customize logging"""
        print(f"[{self.address_string()}] {format % args}")

def run_server():
    server_address = ('localhost', 5148)
    httpd = HTTPServer(server_address, TempC2Handler)
    print(f"Starting temporary C2 server on http://localhost:5148")
    print("Endpoints:")
    print("  POST /api/agent/hello - Agent registration")
    print("  GET  /api/tasking/{agentId} - Get jobs for agent")
    print("  POST /api/job - Receive job results")
    print("\nPress Ctrl+C to stop the server")
    
    try:
        httpd.serve_forever()
    except KeyboardInterrupt:
        print("\nServer stopped.")

if __name__ == "__main__":
    run_server()
