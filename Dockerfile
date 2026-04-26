# Use a lightweight Python image
FROM python:3.11-slim

# Set working directory inside containter 
WORKDIR /app

# Copy requirements first (better caching when rebuilding)
COPY requirements.txt .

# Install dependencies
RUN pip install --no-cache-dir -r requirements.txt

# Copy the rest of the app
COPY . .

# App runs on port 5000
EXPOSE 5000

# Start the app
CMD ["python", "app.py"]
