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
}

export default function Button({ onClick, text, type = ButtonType.Button }: ButtonProps) {
    const [toggleStatus, setToggleStatus] = useState(false);

    function handleClick() {
        switch (type) {
            case ButtonType.Button:
                onClick();
                break;
            case ButtonType.Toggle:
                onClick(!toggleStatus);
                setToggleStatus(!toggleStatus);
                break;
        }
    }

    return (
        <div
            onClick={handleClick}
            className={[
                styles.button,
                type === ButtonType.Toggle && toggleStatus && styles.active,
            ].filter(Boolean).join(' ')}
        >
            {text}
        </div>
    )
}