## Running JavaScript API locally

1. Navigate to the `api/javascript` directory.
1. Fill in the environment variables in the `.env` file. See [Variables](#Variables) below for descriptions.
1. Run `npm install` to install the required dependencies.
1. Run `npm start` to start the server.

## Variables

| Variable | Description | Example value |
| -------- | ----------- | ------------- |
| `PORT` | The port on which the API server will run. | 3000 |
| `DIRECT_LINE_SECRET` | The Direct Line secret issued by Bot Framework. Can be found in the Azure Bot Channels Registration resource after enabling the Direct Line channel. |  |