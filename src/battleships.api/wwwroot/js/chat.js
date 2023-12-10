"use strict";

createGameBoard();
createGameBoardDefinition();

let connection = new signalR.HubConnectionBuilder()
    .withUrl("/matchHub")
    .withAutomaticReconnect([0, 2000, 10000, 20000, 30000]) // Retry times in milliseconds
    .build();

connection.on("GameStateChanged", function (gameState) {
    update(gameState);
});

connection.onreconnecting(error => {
    console.error(`Connection lost due to error "${error}". Reconnecting.`);
    updateConnectionStatus("Reconnecting...", 'warning');
});

connection.onreconnected(connectionId => {
    console.log(`Connection reestablished. Connected with connectionId "${connectionId}".`);
    updateConnectionStatus("Connected", 'success');
});

connection.onclose(error => {
    console.error(`Connection closed due to error "${error}".`);
    updateConnectionStatus("Disconnected - Please try to refresh the page", 'danger');
});

connection.start().then(function () {
    updateConnectionStatus("Connected", 'success');
}).catch(function (err) {
    console.error(err.toString());
    updateConnectionStatus("Failed to connect - Refresh page to retry", 'danger');
});

function updateConnectionStatus(status, statusClass) {
    const statusElement = document.getElementById('connectionStatus');
    statusElement.textContent = status;
    statusElement.className = `status-${statusClass}`;
}

function update(gameState) {
    updateGameInfo(gameState);
    updateGameBoard(gameState);
    updateGameBoardDefinition(gameState);
}

function updateGameInfo(gameState) {
    document.getElementById('game-info-status-token').textContent = gameState.token;
    document.getElementById('game-info-status-map-id').textContent = `${gameState.mapId}`;
    document.getElementById('game-info-status-map').textContent = `${gameState.mapId + 1} / ${gameState.mapCount}`;
    document.getElementById('game-info-status-moves').textContent = `${gameState.moveCount} / ${gameState.totalMoveCount}`;
    document.getElementById('game-info-status-avenger-available').textContent = gameState.avengerAvailable ? 'Yes' : 'No';
    document.getElementById('game-info-status-avenger-used').textContent = gameState.avengerUsed ? 'Yes' : 'No';
    document.getElementById('game-info-status-match-finished').textContent = gameState.matchFinished ? 'Yes' : 'No';
    document.getElementById('game-info-status-game-finished').textContent = gameState.gameFinished ? 'Yes' : 'No';
}

function createGameBoard(gameState) {
    const boardElement = document.getElementById('gameBoard');
    for (let i = 0; i < 12; i++) {
        for (let j = 0; j < 12; j++) {
            const cell = document.createElement('div');
            cell.id = `board-cell-${i}-${j}`;
            cell.classList.add('cell');
            boardElement.appendChild(cell);
        }
    }
}

function createGameBoardDefinition(gameState) {
    const boardElement = document.getElementById('gameBoardDefinition');
    for (let i = 0; i < 12; i++) {
        for (let j = 0; j < 12; j++) {
            const cell = document.createElement('div');
            cell.id = `definition-cell-${i}-${j}`;
            cell.classList.add('cell');
            boardElement.appendChild(cell);
        }
    }
}

function updateGameBoard(gameState) {
    for (let i = 0; i < gameState.rows; i++) {
        for (let j = 0; j < gameState.columns; j++) {
            const cell = document.getElementById(`board-cell-${i}-${j}`);
            const cellValue = gameState.gameField[i * gameState.columns + j];
            cell.className = 'cell'; // Reset classes
            switch (cellValue) {
                case '.':
                    cell.classList.add('water');
                    break;
                case 'X':
                    cell.classList.add('hit');
                    break;
                case '*':
                    cell.classList.add('unknown');
                    break;
            }
        }
    }
}

function updateGameBoardDefinition(gameState) {
    for (let i = 0; i < gameState.rows; i++) {
        for (let j = 0; j < gameState.columns; j++) {
            const cell = document.getElementById(`definition-cell-${i}-${j}`);
            const cellInfo = gameState.gameFieldDefinition[i * gameState.columns + j];
            cell.className = 'cell'; // Reset classes
            if (cellInfo.shipWeight > 0) {
                cell.classList.add('ship');
            } else if (cellInfo.shipWeight < 0) {
                cell.classList.add('ship_hit');
            } else {
                cell.classList.add('water');
            }
        }
    }
}