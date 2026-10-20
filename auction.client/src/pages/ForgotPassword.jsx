import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { requestPasswordReset } from '../services/authService';
import ErrorMessage from '../components/common/ErrorMessage';
import SuccessMessage from '../components/common/SuccessMessage';

const ForgotPassword = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError('');
    setSuccess('');
    if (password !== confirmPassword) {
      setError('Passwords do not match.');
      return;
    }
    setLoading(true);
    try {
      const response = await requestPasswordReset(email, password);
      setSuccess(response.message);
      setEmail('');
      setPassword('');
      setConfirmPassword('');
    } catch (err) {
      setError(
        err.response?.data?.detail ||
          Object.values(err.response?.data?.errors || {}).flat().join(' ') ||
          'Unable to update the password. Please try again.'
      );
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-md-6">
          <div className="card">
            <div className="card-body">
              <h3 className="card-title text-center mb-4">Forgot password</h3>
              {error && <ErrorMessage message={error} />}
              {success && <SuccessMessage message={success} />}
              {!success && (
                <form onSubmit={handleSubmit}>
                  <p>Enter your account email and choose a new password.</p>
                  <div className="alert alert-warning" role="note">
                    This direct password reset is for local development only.
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="reset-email">Email</label>
                    <input
                      id="reset-email"
                      type="email"
                      className="form-control"
                      value={email}
                      onChange={(event) => setEmail(event.target.value)}
                      maxLength={256}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="new-password">New password</label>
                    <input
                      id="new-password"
                      type="password"
                      className="form-control"
                      value={password}
                      onChange={(event) => setPassword(event.target.value)}
                      minLength={6}
                      pattern="(?=.*[A-Z])(?=.*[0-9]).{6,}"
                      title="Use at least 6 characters, including one uppercase letter and one number."
                      required
                    />
                    <div className="form-text">
                      At least 6 characters, one uppercase letter, and one number.
                    </div>
                  </div>
                  <div className="mb-3">
                    <label className="form-label" htmlFor="confirm-password">Confirm new password</label>
                    <input
                      id="confirm-password"
                      type="password"
                      className="form-control"
                      value={confirmPassword}
                      onChange={(event) => setConfirmPassword(event.target.value)}
                      minLength={6}
                      required
                    />
                  </div>
                  <button type="submit" className="btn btn-primary w-100" disabled={loading}>
                    {loading ? 'Updating...' : 'Update password'}
                  </button>
                </form>
              )}
              <div className="text-center mt-3">
                <Link to="/login">Back to login</Link>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPassword;
