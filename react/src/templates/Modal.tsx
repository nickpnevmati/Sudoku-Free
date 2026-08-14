import Button from './Button'
import styles from './Modal.module.css'

interface ModalProps {
    text: string,
    primaryAction: {
        text: string,
        onClick: () => void
    },
    secondaryAction: {
        text: string,
        onClick: () => void
    },
    show: boolean
}

export default function Modal({ text, primaryAction, secondaryAction, show }: ModalProps) {
    return show && (
        <div className={styles.modal_root}>
            {text}
            <div className={styles.modal_buttons}>
                <Button onClick={secondaryAction.onClick} text={secondaryAction.text}/>
                <Button onClick={primaryAction.onClick} text={primaryAction.text}/>
            </div>
        </div>
    )
}