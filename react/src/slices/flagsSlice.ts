import { createSlice } from '@reduxjs/toolkit';

interface FlagsState {
    difficulty: number,
    continueGame: boolean,
}

const initialState: FlagsState = {
    difficulty: 0,
    continueGame: false,
}

const flagsSlice = createSlice({
    name: 'flags',
    initialState,
    reducers: {
        setDifficulty(state, value) { state.difficulty = value.payload },
        setContinue(state, value) { state.continueGame = value.payload },
    }
})

export const { setDifficulty, setContinue } = flagsSlice.actions;
export const flagsReducer = flagsSlice.reducer;