import { memo } from "react";
import { useBridge, props } from "src/bridge";

function Timer() {
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

// Takes no props, so memo cuts it out of every parent re-render. It still updates once a second
// off its own `solveTime` global subscription.
export default memo(Timer)
