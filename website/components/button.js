import styles from './button.module.css';

export default function Button({ children, href }) {
  return (
    <a href={href} className={styles.button}>
      {children}
    </a>
  );
}
