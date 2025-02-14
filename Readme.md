# XML-to-JSON Console App with PostgreSQL (Docker)

## Overview
This project demonstrates how to:
1. Read an XML file.
2. Convert that XML to JSON.
3. Insert the JSON into a PostgreSQL database (in a JSONB column).
4. List existing records from the database.
5. Show the JSON data for a specific record in a nicely formatted way.

## Prerequisites
- **Docker** and **Docker Compose** installed on your machine.
- **.NET SDK 7.0** (if you want to run the project locally outside of Docker).
- A text editor or IDE that supports C# (optional, but recommended).

## Project Files
├─ XmlToJsonConsoleApp/
├─ Dockerfile
├─ docker-compose.yml
├─ init_db.sql
├─ Program.cs
├─ XmlToJsonConsoleApp.csproj
└─ example.xml

- **Dockerfile**: The multi-stage build file for the .NET console app container.
- **docker-compose.yml**: Orchestrates both Postgres and the console app.
- **init_db.sql**: SQL script to create the `json_data` table in the database.
- **Program.cs**: Main code for the console app (includes `run`, `list`, and `show` commands).
- **XmlToJsonConsoleApp.csproj**: The .NET project file.
- **example.xml**: A sample XML file for demo.

## How It Works
1. **Build & Run**: Docker Compose builds the .NET app image and starts a PostgreSQL container.
2. **Initialize DB**: The `init_db.sql` script runs inside the Postgres container to create the required table (`json_data`).
3. **Commands**:
   - `run <xml_file>`: Reads the XML from `<xml_file>`, converts it to JSON, inserts JSON into `json_data`.
   - `list`: Lists IDs of all rows in `json_data`.
   - `show <id>`: Displays the JSON data for the specified record ID in a pretty-printed format.

## Step-by-Step: Running the Demo

1. **Clone or Copy the Project**
   Make sure the files described above are in one folder.

2. **Check `docker-compose.yml`**
   - Confirm the environment variables (database, user, password).
   - By default, the database is named `mydb`, user `myuser`, password `mypass`.

3. **Run Docker Compose**
   From the project directory, run:
   ```bash
   docker-compose build
   docker-compose up
   ```


This will:

- Build the .NET console app image.
- Spin up both db (Postgres) and app (the console app) containers.
- Run the init_db.sql to create the json_data table.

4. Verify Postgres is Running
You should see logs for the Postgres container in the console and a message indicating the table was created.

5. Use the App Commands
The app container automatically runs the command specified in docker-compose.yml. By default, it might run run example.xml once and then stop. To run other commands, do the following steps:

  1. Open a new terminal (leave the Docker containers running).
  2. Get the running app container name (e.g., my-console-app) by running docker ps.
  3. Run a command inside that container:

bash
Copy
docker exec -it my-console-app dotnet XmlToJsonConsoleApp.dll run example.xml
Or you can do:

bash
Copy
docker-compose run app dotnet XmlToJsonConsoleApp.dll run example.xml
This will insert the JSON version of example.xml into the DB.

List the entries:

bash
Copy
docker exec -it my-console-app dotnet XmlToJsonConsoleApp.dll list
You should see the IDs of the inserted rows (e.g., 1, 2, 3...).

Show an entry:

bash
Copy
docker exec -it my-console-app dotnet XmlToJsonConsoleApp.dll show 1
This fetches the JSON for row id=1 and prints it (with indentation).

Check Data in Postgres

Use docker exec -it my-postgres psql -U myuser -d mydb to access the Postgres prompt.
Run SELECT * FROM json_data; to see your rows.
Stop the Containers
Press Ctrl + C in the terminal where docker-compose up is running or run:

bash
Copy
docker-compose down
Notes
You can change the connection string or the Postgres user/password in docker-compose.yml.
You can run the .NET console app locally (outside Docker) as long as you have the right connection string pointing to your Postgres instance.