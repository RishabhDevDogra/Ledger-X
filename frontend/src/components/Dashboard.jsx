import { useState, useEffect } from 'react';
import { apiClient } from '../api/client';
import './Dashboard.css';

export function Dashboard() {
  const [accounts, setAccounts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    fetchAccounts();
  }, []);

  const fetchAccounts = async () => {
    try {
      setLoading(true);
      const data = await apiClient.get('/accounts');
      console.log('Accounts fetched:', data);
      setAccounts(data);
    } catch (err) {
      console.error('Error fetching accounts:', err);
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div className="dashboard"><p>Loading...</p></div>;
  if (error) return <div className="dashboard error"><p>Error: {error}</p><p>Make sure the backend is running on http://localhost:5000</p></div>;

  return (
    <div className="dashboard">
      <h1>LedgerX - Accounts</h1>
      <button onClick={fetchAccounts} className="refresh-btn">
        Refresh
      </button>
      {accounts.length === 0 ? (
        <p>No accounts found</p>
      ) : (
        <table className="accounts-table">
          <thead>
            <tr>
              <th>Account Name</th>
              <th>Account Type</th>
              <th>Balance</th>
            </tr>
          </thead>
          <tbody>
            {accounts.map((account) => (
              <tr key={account.id}>
                <td>{account.name}</td>
                <td>{account.type}</td>
                <td>${account.balance?.toFixed(2) || '0.00'}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
