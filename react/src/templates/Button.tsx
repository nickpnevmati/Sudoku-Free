import { useState } from 'react'
import styles from './Button.module.css'

export enum ButtonType {
    Button,
    Toggle
}

interface ButtonProps {
    onClick: (status?: boolean) => void,
    text: string,
    type?: ButtonType,
    toggleStatus?: boolean,
}

export default function Button({ onClick, text, type = ButtonType.Button, toggleStatus = undefined }: ButtonProps) {
    const [toggleStatusInternal, setToggleStatusInternal] = useState(false);

    const status = toggleStatus ?? toggleStatusInternal;

    function handleClick() {
        switch (type) {
            case ButtonType.Button:
                onClick();
                break;
            case ButtonType.Toggle:
                onClick(!status);
                setToggleStatusInternal(!status);
                break;
        }
    }

    return (
        <div
            onClick={handleClick}
            className={[
                styles.button,
                type === ButtonType.Toggle && status && styles.active,
            ].filter(Boolean).join(' ')}
        >
            {text}
        </div>
    )
}