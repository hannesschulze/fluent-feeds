import Layout from '../../components/layout';
import Head from 'next/head';
import commonStyles from '../../styles/common.module.css';

export default function Blog() {
  return (
    <Layout>
      <Head>
        <title>Blog | Fluent Feeds</title>
      </Head>

      <section className={commonStyles.section}>
        <h1>Blog</h1>
      </section>
    </Layout>
  )
}
