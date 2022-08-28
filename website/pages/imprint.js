import Layout from '../components/layout';
import Head from 'next/head';
import commonStyles from '../styles/common.module.css';
import Link from 'next/link';

export default function PrivacyPolicy() {
  return (
    <Layout>
      <Head>
        <title>Imprint | Fluent Feeds</title>
      </Head>

      <section className={commonStyles.section}>
        <h1>Imprint/<wbr />Impressum</h1>
        
        <p>
          <strong>Angaben nach &sect; 5 TMG</strong><br />
          Hannes Schulze<br />
          Singer Str. 79<br />
          98693 Ilmenau
        </p>

        <p>
          <strong>Kontakt:</strong><br />
          E-Mail: haschu0103(at)gmail.com<br />
        </p>

        <p>
          <strong>Datenschutz:</strong><br />
          <Link href='/privacy-policy'>Zur Datenschutzerklärung</Link>
        </p>
      </section>
    </Layout>
  )
}
