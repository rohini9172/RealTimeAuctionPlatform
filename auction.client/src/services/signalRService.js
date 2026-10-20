import { HubConnectionBuilder } from '@microsoft/signalr';
import { API_BASE_URL } from './api';

let connection = null;

export const createConnection = (auctionId, onNewBid, onAuctionEnded) => {
  const token = localStorage.getItem('token');
  connection = new HubConnectionBuilder()
    .withUrl(`${API_BASE_URL}/auctionHub`, {
      accessTokenFactory: () => token,
    })
    .withAutomaticReconnect()
    .build();

  connection.on('NewBid', (bidUpdate) => {
    if (onNewBid) onNewBid(bidUpdate);
  });

  connection.on('AuctionEnded', (result) => {
    if (onAuctionEnded) onAuctionEnded(result);
  });

  return connection;
};

export const joinAuctionGroup = async (connection, auctionId) => {
  if (connection) {
    await connection.invoke('JoinAuctionGroup', auctionId);
  }
};

export const leaveAuctionGroup = async (connection, auctionId) => {
  if (connection) {
    await connection.invoke('LeaveAuctionGroup', auctionId);
  }
};

export const startConnection = async (connection) => {
  if (connection) {
    await connection.start();
  }
};

export const stopConnection = async (currentConnection = connection) => {
  if (currentConnection) {
    await currentConnection.stop();
    if (connection === currentConnection) {
      connection = null;
    }
  }
};
