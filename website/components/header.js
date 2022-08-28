import Link from 'next/link';
import Head from 'next/head';
import styles from './header.module.css'
import commonStyles from './../styles/common.module.css'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faGithub } from '@fortawesome/free-brands-svg-icons';
import { faBars, faRss } from '@fortawesome/free-solid-svg-icons';
import { useRouter } from 'next/router';
import { useEffect, useState } from 'react';

export default function Header() {
  const router = useRouter();
  const [isScrolled, setScrolled] = useState(false);
  const [isToggled, setToggled] = useState(false);
  const handleScrolled = () => setScrolled(window.scrollY >= 50);
  const handleToggleClicked = () => setToggled(!isToggled);

  useEffect(() => {
    window.addEventListener('scroll', handleScrolled);
    return () => window.removeEventListener('scroll', handleScrolled);
  }, []);

  return (
    <>
      <Head>
        <link rel='icon' href='/fluent-feeds/favicon.ico' />
      </Head>

      <header className={`${styles.header} ${isScrolled ? styles.headerScrolled : ''} ${isToggled ? styles.headerToggled : ''}`}>
        <div className={commonStyles.container}>
          <span className={styles.title}>Fluent Feeds</span>
          
          <nav className={styles.nav}>
            <ul className={styles.navLocal}>
              <li className={router.pathname == '/' ? styles.itemActive : ''}>
                <Link href='/'>Home</Link>
              </li>
              <li className={router.pathname.startsWith('/blog') ? styles.itemActive : ''}>
                <Link href='/blog'>Blog</Link>
              </li>
            </ul>
            <ul className={styles.navExternal}>
              <li>
                <a href='/fluent-feeds/blog/feeds/atom.xml'>
                  <FontAwesomeIcon icon={faRss} />
                </a>
              </li>
              <li>
                <a href='https://github.com/hannesschulze/fluent-feeds'>
                  <FontAwesomeIcon icon={faGithub} />
                </a>
              </li>
            </ul>
          </nav>

          <span className={styles.menuButton} onClick={handleToggleClicked}>
            <FontAwesomeIcon icon={faBars} />
          </span>
        </div>
      </header>
    </>
  );
}
