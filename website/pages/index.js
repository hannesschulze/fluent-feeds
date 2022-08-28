import Layout from '../components/layout';
import Head from 'next/head';
import Button from '../components/button';
import commonStyles from '../styles/common.module.css';
import styles from './index.module.css';

export default function Home() {
  return (
    <Layout>
      <Head>
        <title>Fluent Feeds</title>
      </Head>

      <section className={`${styles.hero} ${commonStyles.section}`}>
        <h1>Fluent Feeds</h1>
        <h2 className='tagline'>A feed reader for Windows 11.</h2>
        <div className={styles.buttons}>
          <Button href='https://github.com/hannesschulze/fluent-feeds/releases'>Download</Button>
        </div>
        <img src='/fluent-feeds/screenshots/main.png' />
      </section>

      <section className={`${styles.features} ${commonStyles.section}`}>
        <div className={styles.featureModular}>
          <img src='/fluent-feeds/screenshots/modular.png' />
          <h2>Modular</h2>
          <p>
            Fluent Feeds is designed around generic &ldquo;feed providers&rdquo;. This makes it easy to implement new
            feed sources that integrate into the app without modifying the structure of the app itself.
          </p>
        </div>

        <div className={styles.featureNative}>
          <img src='/fluent-feeds/screenshots/native.png' />
          <h2>Native</h2>
          <p>
            Fluent Feeds is a native Windows application that leverages the power
            of <a href='https://microsoft.github.io/microsoft-ui-xaml/'>WinUI 3</a> to deliver a fast and modern UI
            that fits into the rest of your system.
          </p>
        </div>

        <div className={styles.featureOpenSource}>
          <h2>Open Source</h2>
          <p>
            The source code is released on GitHub under the MIT license. Anyone can submit pull requests or issues with
            bug reports or feature requests.
          </p>
        </div>
      </section>
    </Layout>
  )
}
