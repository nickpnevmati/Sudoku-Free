import styles from './Toggle.module.css'

interface ToggleProps {
    onChange: (value: boolean) => void,
    value?: boolean,
    offText?: string,
    onText?: string,
}

export default function Toggle({ onChange, value = false, offText, onText }: ToggleProps) {
    const cx = (...names: (string | false | undefined)[]) =>
        names.filter(Boolean).join(' ');

    return (
        <div className={styles.toggle} onClick={() => onChange(!value)}>
            {offText && (
                <div className={cx(styles.label, !value && styles.label_active)}>
                    {offText}
                </div>
            )}

            <div className={cx(styles.track, value && styles.track_on)}>
                <div className={styles.knob} />
            </div>

            {onText && (
                <div className={cx(styles.label, value && styles.label_active)}>
                    {onText}
                </div>
            )}
        </div>
    )
}
