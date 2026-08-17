import { configureStore } from '@reduxjs/toolkit';
import { flagsReducer } from './slices/flagsSlice';

export const store = configureStore({
    reducer: {
        flagsReducer
    },
})

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;