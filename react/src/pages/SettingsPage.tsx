import { useGlobals } from "@reactunity/renderer";
import { ReactNode } from "react";
import { screens } from "src/App";
import Button from "src/templates/Button";
import Toggle from "src/templates/Toggle";
import styles from './SettingsPage.module.css'

export const settings = {
    'darkTheme': 'darkTheme',
    'checkErrors': 'checkErrors',
    'disableQuickNote': 'disableQuickNote'
}

interface SettingProps {
    title: string,
    child: ReactNode,
    hint?: string,
}

function Setting({ title, hint, child }: SettingProps) {
    return (
        <div className={styles.setting_row}>
            <div className={styles.setting_text}>
                <div className={styles.setting_title}>{title}</div>
                {hint && (<div className={styles.setting_hint}>{hint}</div>)}
            </div>
            {child}
        </div>
    )
}

export default function SettingsPage() {
    const globals = useGlobals();

    const darkTheme = globals[settings.darkTheme];
    const checkErrors = globals[settings.checkErrors];
    const disableQuickNote = globals[settings.disableQuickNote];

    return (
        <div className={styles.settings_root}>
            <div className={styles.header}>
                <Button text="Back" onClick={() => globals.navigateTo(screens.MainMenu)} />
                <div className={styles.header_title}>Settings</div>
            </div>

            <div className={styles.list}>
                <Setting
                    title="Theme"
                    hint="Switches the board and menu colours"
                    child={(
                        <Toggle
                            value={darkTheme}
                            onChange={(value) => globals.changeSetting(settings.darkTheme, value)}
                            offText="Light"
                            onText="Dark"
                        />
                    )}
                />
                <Setting
                    title="Check Errors"
                    hint="When ON, errors will show in red"
                    child={(
                        <Toggle
                            onChange={(value) => globals.changeSetting(settings.checkErrors, value)}
                            value={checkErrors}
                        />
                    )}
                />
                <Setting
                    title="Disable Quick Note"
                    hint="Hides the quicknote button"
                    child={(
                        <Toggle
                            onChange={(value) => globals.changeSetting(settings.disableQuickNote, value)}
                            value={disableQuickNote}
                        />
                    )}
                />
            </div>
        </div>
    )
}
