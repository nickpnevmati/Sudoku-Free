import { useBridge, props } from "src/bridge";

export default function Timer() {
    const { getGlobal } = useBridge();
    const timerSeconds = getGlobal(props.solveTime);

    const formattedTimer = () => {
        if (timerSeconds < 0) {
            return "";
        }
        const date = new Date(0)
        date.setSeconds(timerSeconds);
        return date.toISOString().substring(11, 19);
    };

    return (
        <p>{formattedTimer()}</p>
    )
}