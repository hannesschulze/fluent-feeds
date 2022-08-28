import Link from 'next/link';
import commonStyles from '../styles/common.module.css';
import styles from './footer.module.css';

export default function Footer() {
  return (
    <footer>
      <div className={commonStyles.section}>
        <div className={`${commonStyles.container} ${styles.footer}`}>
          <span className={styles.copyright}>
            &copy; 2022, the Fluent Feeds developers
          </span>
          <nav className={styles.nav}>
            <ul>
              <li>
                <Link href='/imprint'>Imprint</Link>
              </li>
              <li>
                <Link href='/privacy-policy'>Privacy Policy</Link>
              </li>
            </ul>
          </nav>
        </div>
      </div>
    </footer>
  );
}
