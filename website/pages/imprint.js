import Layout from '../components/layout';
import Head from 'next/head';
import commonStyles from '../styles/common.module.css';

export default function PrivacyPolicy() {
  return (
    <Layout>
      <Head>
        <title>Imprint | Fluent Feeds</title>
      </Head>

      <section className={commonStyles.section}>
        <h1>Imprint</h1>
      </section>
    </Layout>
  )
}
