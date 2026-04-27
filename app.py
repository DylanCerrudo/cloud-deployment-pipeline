from flask import Flask, jsonify
import logging
import os
from datetime import datetime, timezone

# Initialize Flask app
app = Flask(__name__)

# Configure basic logging(later integrates with CloudWatch in AWS)
logging.basicConfig(level=logging.INFO)

# Root endpoint - used to verify service is running
@app.route("/")
def home():
    # Log incoming request (important for monitoring/debugging)
    app.logger.info("Home endpoint was called")
    
    return jsonify({
        "message": "Cloud Deployment Automation Pipeline is running",
        "status": "healthy",
        "timestamp": datetime.now(timezone.utc).isoformat()
        
    })

# Health check endpoint - used by load balancers / monitoring tools
@app.route("/health")
def health():
    app.logger.info("Health check endpoint was called")
    
    return jsonify({
        "status": "ok",
        "service": "python-api"
    })


if __name__ == "__main__":
    # Use environment variable for port (required for Docker/AWS compatibility)
    port = int(os.environ.get("PORT", 80))
    
    # Bind to 0.0.0.0 so the app is accesible outside the container/VM
    app.run(host="0.0.0.0", port=port)