import React, { createContext, useContext, useState, useCallback, useMemo } from 'react';
import LoadingOverlay from '../components/LoadingOverlay';

interface LoadingContextType {
  showLoading: (message?: string) => void;
  hideLoading: () => void;
  isLoading: boolean;
}

const LoadingContext = createContext<LoadingContextType | undefined>(undefined);

export const LoadingProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [loadingCount, setLoadingCount] = useState(0);
  const [message, setMessage] = useState<string | undefined>(undefined);

  const showLoading = useCallback((msg?: string) => {
    setLoadingCount((count) => count + 1);
    setMessage(msg);
  }, []);

  const hideLoading = useCallback(() => {
    setLoadingCount((count) => Math.max(0, count - 1));
  }, []);

  const isLoading = loadingCount > 0;

  const contextValue = useMemo(() => ({
    showLoading,
    hideLoading,
    isLoading
  }), [showLoading, hideLoading, isLoading]);

  return (
    <LoadingContext.Provider value={contextValue}>
      {children}
      {isLoading && <LoadingOverlay message={message} />}
    </LoadingContext.Provider>
  );
};

export const useLoading = () => {
  const context = useContext(LoadingContext);
  if (context === undefined) {
    throw new Error('useLoading must be used within a LoadingProvider');
  }
  return context;
};
