# Certificate Generation System - Traof

A professional ASP.NET Core system for generating certificates from image templates.

## Features
- Upload Image Template
- Drag and Drop text positioning
- Multi-source data: Local Employee Database or Excel file upload
- Export as PNG, JPG, or PDF
- Arabic font (Cairo) support included

## 🚀 Live Deployment (Render)

[![Deploy to Render](https://render.com/images/deploy-to-render-button.svg)](https://render.com/deploy?repo=https://github.com/roco19866/cr)

To host this project on the web so anyone can use it:

1. Create a free account on [Render.com](https://render.com).
2. Click **New +** and select **Web Service**.
3. Connect your GitHub account and select this repository (`cr`).
4. **Settings**:
   - **Runtime**: `Docker`
   - **Free Plan**: Selected
5. Click **Deploy Web Service**.

Render will automatically build the `Dockerfile` and give you a public link!

## Local Run
```bash
dotnet run
```
Access at `http://localhost:5202`
