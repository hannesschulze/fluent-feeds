import Header from './header';
import Footer from './footer';
import commonStyles from '../styles/common.module.css';
import styles from './layout.module.css';

export default function Layout({ children }) {
  return (
    <>
      <Header />
      
      <main className={styles.main}>
        <div className={commonStyles.container}>
          {children}
        </div>
      </main>

      <Footer />
    </>
  );
}
