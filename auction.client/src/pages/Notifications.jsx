import React, { useState, useEffect } from 'react';
import { getNotifications, markNotificationAsRead } from '../services/notificationService';
import { useAuth } from '../context/AuthContext';
import Loading from '../components/common/Loading';
import ErrorMessage from '../components/common/ErrorMessage';

const Notifications = () => {
  const { user } = useAuth();
  const [notifications, setNotifications] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  useEffect(() => {
    loadNotifications();
  }, []);

  const loadNotifications = async () => {
    try {
      setLoading(true);
      const data = await getNotifications();
      setNotifications(data);
    } catch (err) {
      setError('Failed to load notifications.');
    } finally {
      setLoading(false);
    }
  };

  const handleMarkAsRead = async (id) => {
    try {
      await markNotificationAsRead(id);
      setNotifications((prev) =>
        prev.map((n) =>
          n.id === id ? { ...n, isRead: true } : n
        )
      );
    } catch (err) {
      setError('Failed to mark notification as read.');
    }
  };

  const typeColor = {
    AuctionStarted: 'info',
    AuctionEnded: 'secondary',
    OutBid: 'warning',
    AuctionWon: 'success',
    AuctionCancelled: 'danger',
  };

  if (loading) return <Loading />;

  return (
    <div className="container mt-4">
      {error && <ErrorMessage message={error} />}
      <h2 className="mb-4">My Notifications</h2>
      {notifications.length === 0 ? (
        <div className="text-center py-4">No notifications.</div>
      ) : (
        <div className="list-group">
          {notifications.map((n) => (
            <div
              key={n.id}
              className={`list-group-item list-group-item-action ${
                !n.isRead ? 'bg-light' : ''
              }`}
              onClick={() => handleMarkAsRead(n.id)}
            >
              <div className="d-flex w-100 justify-content-between align-items-center">
                <span
                  className={`badge me-2 bg-${typeColor[n.type] || 'dark'}`}
                >
                  {n.type}
                </span>
                <span className="text-muted small">
                  {new Date(n.createdAtUtc).toLocaleString()}
                </span>
              </div>
              <div className="mt-2">{n.message}</div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};

export default Notifications;
