import React from 'react';

const LoadingOverlay: React.FC<{ message?: string }> = ({ message = "Syncing Timeline..." }) => {
  return (
    <div className="fixed inset-0 z-[9999] flex flex-col items-center justify-center bg-background/80 backdrop-blur-md">
      <div className="relative h-24 w-24">
        {/* Outer Ring */}
        <div className="absolute inset-0 animate-spin rounded-full border-4 border-primary/30 border-t-primary shadow-[0_0_15px_rgba(0,244,254,0.3)]"></div>
        {/* Inner Ring */}
        <div className="absolute inset-4 animate-[spin_1.5s_linear_infinite_reverse] rounded-full border-4 border-secondary/30 border-t-secondary shadow-[0_0_10px_rgba(97,180,254,0.3)]"></div>
        {/* Center Glow */}
        <div className="absolute inset-0 flex items-center justify-center">
            <div className="h-2 w-2 rounded-full bg-primary shadow-[0_0_20px_#00f4fe]"></div>
        </div>
      </div>
      <p className="mt-8 font-headline font-bold tracking-[0.2em] text-on-background animate-pulse uppercase">
        {message}
      </p>
    </div>
  );
};

export default LoadingOverlay;
